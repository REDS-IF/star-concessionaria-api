using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Contatos;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/contatos")]
public class ContatosController : ControllerBase
{
    private const string PerfisComercial = "Administrador,Comercial";
    private readonly IContatoService _service;

    public ContatosController(IContatoService service) => _service = service;

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<ContatoResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ContatoResponse>> Criar([FromBody] NovoContatoRequest request, CancellationToken ct)
    {
        var r = await _service.CriarContatoAsync(request, ct);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r);
    }

    [HttpPost("vendedor")]
    [AllowAnonymous]
    [ProducesResponseType<ContatoResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ContatoResponse>> EncaminharVendedor([FromBody] ContatoVendedorRequest request, CancellationToken ct)
    {
        var r = await _service.EncaminharVendedorAsync(request, ct);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r);
    }

    [HttpGet]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<PagedResult<ContatoResponse>>> Listar([FromQuery] ContatoQuery filtro, CancellationToken ct) =>
        Ok(await _service.ListarAsync(filtro, ct));

    [HttpPatch("{id:int}/responder")]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<ContatoResponse>> Responder(int id, [FromBody] ResponderContatoRequest request, CancellationToken ct)
    {
        var usuario = new UsuarioAtual(
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Name) ?? "desconhecido");
        var r = await _service.ResponderAsync(id, request, usuario, ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    private ActionResult Erro<T>(Resultado<T> r) => r.Erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new ProblemDetails { Status = 404, Title = r.Mensagem }),
        TipoErro.Conflito => Conflict(new ProblemDetails { Status = 409, Title = r.Mensagem }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = r.Mensagem })
    };
}
