using MongoDB.Driver;

namespace Corporativo.RendaVariavel.Infrascructure.BancoDeDados;

public sealed class BancoDeDadosConfiguracao
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoDatabase _database;

    public BancoDeDadosConfiguracao(string textoDeConexao, string bancoDeDados)
    {
        _mongoClient = new MongoClient(textoDeConexao);
        _database = _mongoClient.GetDatabase(bancoDeDados);
    }

    public IMongoCollection<T> ObterColecaoDoBancoDeDados<T>(string nomeDaColecao)
        => _database.GetCollection<T>(nomeDaColecao);
}
