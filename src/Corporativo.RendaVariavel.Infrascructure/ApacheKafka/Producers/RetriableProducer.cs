using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Corporativo.RendaVariavel.Infrascructure.ApacheKafka.Producers;

public class RetriableProducer<T> : IRetriableProducer<T>, IProducer<T>
{
    protected readonly ILogger<RetriableProducer<T>> _logger;
    protected readonly IProducer<string, string> _producer;
    protected readonly ProducerConfiguration _configuration;

    public RetriableProducer(ILogger<RetriableProducer<T>> logger, IProducer<string, string> producer, ProducerConfiguration configuration)
    {
        _logger = logger;
        _producer = producer;
        _configuration = configuration;
    }

    public async Task ProduceAsync(string key, T message, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(message);

            var @event = new Message<string, string>()
            {
                Key = key,
                Value = json
            };

            var result = await _producer.ProduceAsync(
                topic: _configuration.TopicName,
                message: @event,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                exception: ex,
                message: "[{Type}][{Method}] An unhandled exception has been throwed producing apache kafka message. Info = {@Info}",
                nameof(RetriableProducer<T>),
                nameof(ProduceAsync),
                new
                {
                    Key = key,
                });
        }
    }

    public async Task ProduceRetriableAsync(string key, T message, CancellationToken cancellationToken = default)
    {
        var retriableConfiguration = _configuration.Retriable;

        var timeoutConfiguration = _configuration.Timeout;

        try
        {
            AsyncRetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: retriableConfiguration.RetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(retriableConfiguration.DelayInMiliseconds),
                    onRetry: (exception, delay, retryCount, context) =>
                    {
                        _logger?.LogWarning(
                            exception: exception,
                            "[{Type}][{Method}] Try {Retry} has been failed. Resending by {Delay}ms. RetriableConfiguration = {@RetriableConfiguration}; TopicName = {TopicName} Key = {Key};",
                            nameof(RetriableProducer<T>),
                            nameof(ProduceRetriableAsync),
                            retryCount,
                            delay,
                            retriableConfiguration,
                            _configuration.TopicName,
                            key);
                    });

            AsyncTimeoutPolicy timeoutPolicy =
                Policy.TimeoutAsync(
                    seconds: timeoutConfiguration.Seconds,
                    timeoutStrategy: TimeoutStrategy.Optimistic,
                    onTimeoutAsync: (context, timespan, task, ex) =>
                    {
                        _logger?.LogError(
                            "[{Type}][{Method}] Timeout after {Timeout}ms. Topic = {Topic}; Key = {Key}",
                            nameof(RetriableProducer<T>),
                            nameof(ProduceRetriableAsync),
                            timespan.TotalMilliseconds,
                            _configuration.TopicName,
                            key);

                        return Task.CompletedTask;
                    });

            var wrapPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy);

            await wrapPolicy.ExecuteAsync(async (stoppingToken) =>
            {
                var json = JsonSerializer.Serialize(message);

                var @event = new Message<string, string>()
                {
                    Key = key,
                    Value = json
                };

                var result = await _producer.ProduceAsync(_configuration.TopicName, @event, stoppingToken);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                exception: ex,
                message: "[{Type}][{Method}] All retries has been failed. RetriableConfiguration = {@RetriableConfiguration}; TopicName = {TopicName} Key = {Key};",
                nameof(RetriableProducer<T>),
                nameof(ProduceRetriableAsync),
                retriableConfiguration,
                _configuration.TopicName,
                key);
        }
    }
}
