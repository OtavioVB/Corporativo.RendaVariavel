using Confluent.Kafka;

namespace Corporativo.RendaVariavel.Infrascructure.ApacheKafka.Producers;

public sealed record ProducerConfiguration
{
    public ProducerConfig ProducerConfig { get; set; } = new ProducerConfig();
    public RetriableProducerConfiguration Retriable { get; set; } = new RetriableProducerConfiguration();
    public string TopicName { get; set; } = string.Empty;
}

public sealed record RetriableProducerConfiguration
{
    public bool IsRetriable { get; set; } = false;
    public int RetryCount { get; set; } = 0;
    public int DelayInMiliseconds { get; set; } = 0;
}
