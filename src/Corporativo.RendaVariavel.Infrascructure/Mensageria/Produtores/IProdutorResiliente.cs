namespace Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;

public interface IProdutorResiliente<T>
{
    public Task ProduzirMensagemComRetentativaAssincrona(string key, T message, CancellationToken cancellationToken = default);
}
