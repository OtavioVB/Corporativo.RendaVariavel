using MongoDB.Driver;

namespace Corporativo.RendaVariavel.Infrascructure.BancoDeDados.Repositorios;

public abstract class RepositorioBase<T> : IRepositorio<T>
{
    protected readonly IMongoCollection<T> _colecao;

    protected RepositorioBase(BancoDeDadosConfiguracao configuracao, string nomeDaColecao)
    {
        _colecao = configuracao.ObterColecaoDoBancoDeDados<T>(nomeDaColecao);
    }

    public virtual Task InserirAssincrono(T documento, CancellationToken tokenDeCancelamento = default)
        => _colecao.InsertOneAsync(
            document: documento,
            options: new InsertOneOptions() { BypassDocumentValidation = true },
            cancellationToken: tokenDeCancelamento);
}
