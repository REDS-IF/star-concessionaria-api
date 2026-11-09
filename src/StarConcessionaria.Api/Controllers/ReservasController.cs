using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Reservas;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/reservas")]
public class ReservasController : ControllerBase
{
    private const string PerfisComercial = "Administrador,Comercial";
    private readonly IReservaService _service;

    public ReservasController(IReservaService service) => _service = service;

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<ReservaResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ReservaResponse>> Solicitar([FromBody] SolicitarReservaRequest request, CancellationToken ct)
    {
        var r = await _service.SolicitarAsync(request, ct);
        return r.Sucesso ? CreatedAtAction(nameof(Obter), new { id = r.Valor!.Id }, r.Valor) : Erro(r);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<ReservaResponse>> Obter(int id, CancellationToken ct)
    {
        var r = await _service.ObterAsync(id, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<PagedResult<ReservaResponse>>> Listar([FromQuery] ReservaQuery filtro, CancellationToken ct) =>
        Ok(await _service.ListarAsync(filtro, ct));

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<ReservaResponse>> AtualizarStatus(int id, [FromBody] AtualizarStatusReservaRequest request, CancellationToken ct)
    {
        var usuario = new UsuarioAtual(
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Name) ?? "desconhecido");
        var r = await _service.AtualizarStatusAsync(id, request, usuario, ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    private ActionResult Erro<T>(Resultado<T> r) => r.Erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new ProblemDetails { Status = 404, Title = r.Mensagem }),
        TipoErro.Conflito => Conflict(new ProblemDetails { Status = 409, Title = r.Mensagem }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = r.Mensagem })
    };
}
