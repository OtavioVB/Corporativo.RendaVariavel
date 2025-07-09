namespace Corporativo.RendaVariavel.Infrascructure.BancoDeDados.Repositorios;

public interface IRepositorio<T>
{
    public Task InserirAssincrono(T documento, CancellationToken tokenDeCancelamento = default);
}
