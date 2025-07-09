using Corporativo.RendaVariavel.Domain.BoundedContexts.CustomerContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;

public static class RegistroDeInjecaoDeDependencia
{
    public static IServiceCollection RegistrarInjecaoDeDependenciaDeProducaoDeEventos(
        this IServiceCollection colecaoDeServicos, IConfiguration configurador)
    {
        var produtorDeMensagemConfiguracao = configurador
            .GetRequiredSection("Mensageria:ApacheKafka:ProdutorDeMensagemConfiguracao")
            .Get<ProdutorDeMensagemConfiguracao>()!;

        colecaoDeServicos.AddSingleton<IProdutorResiliente<Cliente>, ProdutorResiliente<Cliente>>(provedorDeServico 
            => new ProdutorResiliente<Cliente>(
                logger: provedorDeServico.GetRequiredService<ILogger<ProdutorResiliente<Cliente>>>(),
                configuracao: produtorDeMensagemConfiguracao));

        return colecaoDeServicos;
    }
}
