using MongoDB.Bson;

namespace Corporativo.RendaVariavel.Domain.BoundedContexts.CustomerContext;

public sealed record Cliente
{
    public Cliente(string nome, string sobrenome, DateTime dataDeNascimento, string caixaEletronico)
    {
        Nome = nome;
        Sobrenome = sobrenome;
        DataDeNascimento = dataDeNascimento;
        CaixaEletronico = caixaEletronico;
    }

    public string Nome { get; set; }
    public string Sobrenome { get; set; }
    public DateTime DataDeNascimento { get; set; }
    public string CaixaEletronico { get; set; }
}
