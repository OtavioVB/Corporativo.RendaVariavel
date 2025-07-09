using Confluent.Kafka;
using Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;
using Microsoft.Extensions.Logging;
using Moq;

namespace Corporativo.RendaVariavel.UnitTests.Infrascructure.ApacheKafka;

public sealed class RetriableProducerTests
{
    private readonly Mock<ILogger<ProdutorResiliente<RetriableProducerFakeObject>>> _mockLogger = new();
    private readonly Mock<IProducer<string, string>> _mockKafkaProducer = new();

    public sealed record RetriableProducerFakeObject
    {
        public string Message { get; set; } = string.Empty;
    }

    private ProdutorResiliente<RetriableProducerFakeObject> CreateProducer(ProdutorDeMensagemConfiguracao config)
        => new ProdutorResiliente<RetriableProducerFakeObject>(
            _mockLogger.Object,
            _mockKafkaProducer.Object,
            config);

    [Fact]
    public async Task GivenRequestRetriableProduceAsync_ShouldCallProducerAsExpected()
    {
        // Arrange
        var config = new ProdutorDeMensagemConfiguracao { NomeDoTopico = "test-topic" };

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
        await producer.ProduzirMensagemAssincrona(key, message, CancellationToken.None);

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
        var config = new ProdutorDeMensagemConfiguracao
        {
            NomeDoTopico = "retry-topic",
            Retentativa = new RetentativaConfiguracao
            {
                NumeroDeRetentativas = 3,
                IntervaloEmMilisegundos = 1
            },
            TempoEsgotado = new TempoEsgotadoConfiguracao
            {
                Segundos = 5
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
        await producer.ProduzirMensagemComRetentativaAssincrona(key, message);

        // Assert
        Assert.Equal(config.Retentativa.NumeroDeRetentativas + 1, attempts);
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
        var config = new ProdutorDeMensagemConfiguracao
        {
            NomeDoTopico = "retry-topic",
            Retentativa = new RetentativaConfiguracao
            {
                NumeroDeRetentativas = 2,
                IntervaloEmMilisegundos = 1
            },
            TempoEsgotado = new TempoEsgotadoConfiguracao
            {
                Segundos = 5
            }
        };

        var producer = CreateProducer(config);
        var key = "retry-key";
        var message = new RetriableProducerFakeObject() { Message = "error-retry-message" };

        _mockKafkaProducer
            .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Kafka error"));

        // Act
        await producer.ProduzirMensagemComRetentativaAssincrona(key, message);

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
        var config = new ProdutorDeMensagemConfiguracao
        {
            NomeDoTopico = "timeout-topic",
            Retentativa = new RetentativaConfiguracao
            {
                NumeroDeRetentativas = 1,
                IntervaloEmMilisegundos = 1
            },
            TempoEsgotado = new TempoEsgotadoConfiguracao
            {
                Segundos = 1
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

        var producer = new ProdutorResiliente<RetriableProducerFakeObject>(_mockLogger.Object, producerMock.Object, config);

        var key = "timeout-key";
        var message = new RetriableProducerFakeObject() { Message = "error-retry-message" };

        // Act
        await producer.ProduzirMensagemComRetentativaAssincrona(key, message, cancellationToken);

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
