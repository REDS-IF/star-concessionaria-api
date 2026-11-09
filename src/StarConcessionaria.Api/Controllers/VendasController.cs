using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;
using StarConcessionaria.Application.Vendas;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/vendas")]
public class VendasController : ControllerBase
{
    private const string PerfisComercial = "Administrador,Comercial";
    private readonly IVendaService _service;

    public VendasController(IVendaService service) => _service = service;

    [HttpGet]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<PagedResult<VendaResponse>>> Listar([FromQuery] VendaQuery filtro, CancellationToken ct) =>
        Ok(await _service.ListarAsync(filtro, ct));

    [HttpPost]
    [Authorize(Roles = PerfisComercial)]
    [ProducesResponseType<VendaResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<VendaResponse>> Registrar([FromBody] RegistrarVendaRequest request, CancellationToken ct)
    {
        var usuario = new UsuarioAtual(
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Name) ?? "desconhecido");
        var r = await _service.RegistrarAsync(request, usuario, ct);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r);
    }

    private ActionResult Erro<T>(Resultado<T> r) => r.Erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new ProblemDetails { Status = 404, Title = r.Mensagem }),
        TipoErro.Conflito => Conflict(new ProblemDetails { Status = 409, Title = r.Mensagem }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = r.Mensagem })
    };
}
