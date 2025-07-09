using Corporativo.RendaVariavel.Domain.BoundedContexts.CustomerContext;
using Corporativo.RendaVariavel.Infrascructure.BancoDeDados.Repositorios;
using Corporativo.RendaVariavel.Infrascructure.Mensageria.Produtores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Corporativo.RendaVariavel.Api.Controllers;

[Route("api/v1/customers")]
[ApiController]
public sealed class CustomerController : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> HttpPostCreateCustomerAsync(
        [FromServices] IRepositorio<Cliente> repositorio,
        [FromServices] IProdutorResiliente<Cliente> produtor,
        [FromBody] Cliente cliente,
        CancellationToken cancellationToken)
    {
        await repositorio.InserirAssincrono(cliente, cancellationToken);

        await produtor.ProduzirMensagemComRetentativaAssincrona(
            key: $"{cliente.Nome}#{cliente.Sobrenome}",
            message: cliente,
            cancellationToken: cancellationToken);

        return StatusCode(StatusCodes.Status204NoContent);
    }
}
