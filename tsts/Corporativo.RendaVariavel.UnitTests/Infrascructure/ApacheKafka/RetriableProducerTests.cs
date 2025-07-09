using Confluent.Kafka;
using Corporativo.RendaVariavel.Infrascructure.ApacheKafka.Producers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Corporativo.RendaVariavel.UnitTests.Infrascructure.ApacheKafka;

public sealed class RetriableProducerTests
{
    private readonly Mock<ILogger<RetriableProducer<RetriableProducerFakeObject>>> _mockLogger = new();
    private readonly Mock<IProducer<string, string>> _mockKafkaProducer = new();

    public sealed record RetriableProducerFakeObject
    {
        public string Message { get; set; } = string.Empty;
    }

    private RetriableProducer<RetriableProducerFakeObject> CreateProducer(ProducerConfiguration config)
        => new RetriableProducer<RetriableProducerFakeObject>(
            _mockLogger.Object,
            _mockKafkaProducer.Object,
            config);

    [Fact]
    public async Task GivenRequestRetriableProduceAsync_ShouldCallProducerAsExpected()
    {
        // Arrange
        var config = new ProducerConfiguration { TopicName = "test-topic" };

        var producer = CreateProducer(config);
        var key = "my-key";
        var message = new RetriableProducerFakeObject() { Message = "my-message" };

        _mockKafkaProducer
            .Setup(p => p.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<Message<string, string>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        // Act
        await producer.ProduceAsync(key, message, CancellationToken.None);

        // Assert
        _mockKafkaProducer.Verify(p =>
            p.ProduceAsync(
                It.Is<string>(p => p == "test-topic"),
                It.Is<Message<string, string>>(m => m.Key == key && m.Value.Contains("my-message")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenRequestRetriableProduceAsyncThrowsAnyError_ShouldCallRetryingProducerUntilMaxAttempts()
    {
        // Arrange
        var config = new ProducerConfiguration
        {
            TopicName = "retry-topic",
            Retriable = new RetriableProducerConfiguration
            {
                RetryCount = 3,
                DelayInMiliseconds = 1
            },
            Timeout = new TimeoutProducerConfiguration
            {
                Seconds = 5
            }
        };

        var producer = CreateProducer(config);
        var key = "retry-key";
        var message = new RetriableProducerFakeObject() { Message = "retry-message" };
        var attempts = 0;

        _mockKafkaProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback(() => attempts++)
            .ThrowsAsync(new Exception("Kafka is down"));

        // Act
        await producer.ProduceRetriableAsync(key, message);

        // Assert
        Assert.Equal(config.Retriable.RetryCount + 1, attempts);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("has been failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GivenRequestRetriableProductAsyncThrowsAnyError_ShouldCallAllAttemptsAndLogErrorAsExpected()
    {
        // Arrange
        var config = new ProducerConfiguration
        {
            TopicName = "retry-topic",
            Retriable = new RetriableProducerConfiguration
            {
                RetryCount = 2,
                DelayInMiliseconds = 1
            },
            Timeout = new TimeoutProducerConfiguration
            {
                Seconds = 5
            }
        };

        var producer = CreateProducer(config);
        var key = "retry-key";
        var message = new RetriableProducerFakeObject() { Message = "error-retry-message" };

        _mockKafkaProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Kafka error"));

        // Act
        await producer.ProduceRetriableAsync(key, message);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("All retries has been failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GivenRequestRetriableProduceAsyncTimeExecutionIsGreaterThanTheTimeout_ShouldLogTimeoutError()
    {
        // Arrange
        var config = new ProducerConfiguration
        {
            TopicName = "timeout-topic",
            Retriable = new RetriableProducerConfiguration
            {
                RetryCount = 1,
                DelayInMiliseconds = 1
            },
            Timeout = new TimeoutProducerConfiguration
            {
                Seconds = 1
            }
        };

        var producerMock = new Mock<IProducer<string, string>>();

        var cancellationToken = new CancellationToken();

        producerMock
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(async (string topic, Message<string, string> message, CancellationToken token) =>
            {
                await Task.Delay(2000, token);
                return new DeliveryResult<string, string>();
            });

        var producer = new RetriableProducer<RetriableProducerFakeObject>(_mockLogger.Object, producerMock.Object, config);

        var key = "timeout-key";
        var message = new RetriableProducerFakeObject() { Message = "error-retry-message" };

        // Act
        await producer.ProduceRetriableAsync(key, message, cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Timeout")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
