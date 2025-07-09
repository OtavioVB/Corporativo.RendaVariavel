using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using System.Text.Json;

namespace Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;

public class ProdutorResiliente<T> : IRetentativaProdutor<T>, IProdutor<T>
{
    protected readonly ILogger<ProdutorResiliente<T>> _logger;
    protected readonly IProducer<string, string> _producer;
    protected readonly ProdutorDeMensagemConfiguracao _configuration;

    public ProdutorResiliente(ILogger<ProdutorResiliente<T>> logger, IProducer<string, string> producer, ProdutorDeMensagemConfiguracao configuration)
    {
        _logger = logger;
        _producer = producer;
        _configuration = configuration;
    }

    public async Task ProduzirMensagemAssincrona(string key, T message, CancellationToken cancellationToken = default)
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
                topic: _configuration.NomeDoTopico,
                message: @event,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                exception: ex,
                message: "[{Type}][{Method}] An unhandled exception has been throwed producing apache kafka message. Info = {@Info}",
                nameof(ProdutorResiliente<T>),
                nameof(ProduzirMensagemAssincrona),
                new
                {
                    Key = key,
                });
        }
    }

    public async Task ProduzirMensagemComRetentativaAssincrona(string key, T message, CancellationToken cancellationToken = default)
    {
        var retriableConfiguration = _configuration.Retentativa;

        var timeoutConfiguration = _configuration.TempoEsgotado;

        try
        {
            AsyncRetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: retriableConfiguration.NumeroDeRetentativas,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(retriableConfiguration.IntervaloEmMilisegundos),
                    onRetry: (exception, delay, retryCount, context) =>
                    {
                        _logger?.LogWarning(
                            exception: exception,
                            "[{Type}][{Method}] Try {Retry} has been failed. Resending by {Delay}ms. RetriableConfiguration = {@RetriableConfiguration}; TopicName = {TopicName} Key = {Key};",
                            nameof(ProdutorResiliente<T>),
                            nameof(ProduzirMensagemComRetentativaAssincrona),
                            retryCount,
                            delay,
                            retriableConfiguration,
                            _configuration.NomeDoTopico,
                            key);
                    });

            AsyncTimeoutPolicy timeoutPolicy =
                Policy.TimeoutAsync(
                    seconds: timeoutConfiguration.Segundos,
                    timeoutStrategy: TimeoutStrategy.Optimistic,
                    onTimeoutAsync: (context, timespan, task, ex) =>
                    {
                        _logger?.LogError(
                            "[{Type}][{Method}] Timeout after {Timeout}ms. Topic = {Topic}; Key = {Key}",
                            nameof(ProdutorResiliente<T>),
                            nameof(ProduzirMensagemComRetentativaAssincrona),
                            timespan.TotalMilliseconds,
                            _configuration.NomeDoTopico,
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

                var result = await _producer.ProduceAsync(_configuration.NomeDoTopico, @event, stoppingToken);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                exception: ex,
                message: "[{Type}][{Method}] All retries has been failed. RetriableConfiguration = {@RetriableConfiguration}; TopicName = {TopicName} Key = {Key};",
                nameof(ProdutorResiliente<T>),
                nameof(ProduzirMensagemComRetentativaAssincrona),
                retriableConfiguration,
                _configuration.NomeDoTopico,
                key);
        }
    }
}
