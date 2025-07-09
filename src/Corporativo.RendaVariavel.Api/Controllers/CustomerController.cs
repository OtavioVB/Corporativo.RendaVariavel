using Corporativo.RendaVariavel.Domain.BoundedContexts.CustomerContext;
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
        [FromBody] Cliente customer,
        CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status204NoContent);
    }
}
