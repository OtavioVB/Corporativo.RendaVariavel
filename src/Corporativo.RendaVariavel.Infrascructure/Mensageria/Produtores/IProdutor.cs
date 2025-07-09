namespace Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;

public interface IProdutor<T>
{
    public Task ProduzirMensagemAssincrona(string key, T message, CancellationToken cancellationToken = default);
}
