using Corporativo.RendaVariavel.Domain.BoundedContexts.CustomerContext;
using Corporativo.RendaVariavel.Infrascructure.BancoDeDados.Repositorios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corporativo.RendaVariavel.Infrascructure.BancoDeDados;

public static class RegistroInjecaoDeDependencia
{
    public static IServiceCollection RegistrarInjecaoDeDependenciaDoBancoDeDados(this IServiceCollection colecaoDeServicos, IConfiguration configurador)
    {
        var textoDeConexao = configurador["BancoDeDados:TextoDeConexao"]!;
        var bancoDeDados = configurador["BancoDeDados:Nome"]!;

        var configuracao = new BancoDeDadosConfiguracao(
            textoDeConexao: textoDeConexao,
            bancoDeDados: bancoDeDados);

        colecaoDeServicos.AddSingleton<IRepositorio<Cliente>, ClienteRepositorio>(p => new(configuracao));

        return colecaoDeServicos;
    }
}
