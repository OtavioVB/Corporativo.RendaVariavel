namespace Corporativo.RendaVariavel.Infrascructure.ApacheKafka.Producers;

public interface IRetriableProducer<T>
{
    public Task ProduceRetriableAsync(string key, T message, CancellationToken cancellationToken = default);
}
