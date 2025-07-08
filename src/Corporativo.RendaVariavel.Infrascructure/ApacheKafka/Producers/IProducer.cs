namespace Corporativo.RendaVariavel.Infrascructure.ApacheKafka.Producers;

public interface IProducer<T>
{
    public Task ProduceAsync(string key, T message, CancellationToken cancellationToken = default);
}
