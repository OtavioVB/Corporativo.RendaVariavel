using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;

public static class RegistroDeInjecaoDeDependencia
{
    public static IServiceCollection RegistrarInjecaoDeDependenciaDeProducaoDeEventos(
        this IServiceCollection colecaoDeServicos, IConfiguration configurador)
    {
        var produtorDeMensagemConfiguracao = configurador
            .GetRequiredSection("Mensageria:ApacheKafka:ProdutorDeMensagemConfiguracao")
            .Get<ProdutorDeMensagemConfiguracao>()!;

        return colecaoDeServicos;
    }
}
