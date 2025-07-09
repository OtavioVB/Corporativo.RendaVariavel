using Corporativo.RendaVariavel.Domain.BoundedContexts.CustomerContext;

namespace Corporativo.RendaVariavel.Infrascructure.BancoDeDados.Repositorios;

public sealed class ClienteRepositorio : RepositorioBase<Cliente>
{
    public ClienteRepositorio(BancoDeDadosConfiguracao configuracao) : base(configuracao, "CLIENTES")
    {

    }
}
