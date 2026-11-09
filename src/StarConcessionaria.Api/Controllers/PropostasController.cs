using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Contatos;
using StarConcessionaria.Application.Propostas;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/propostas")]
public class PropostasController : ControllerBase
{
    private const string PerfisComercial = "Administrador,Comercial";
    private readonly IPropostaService _service;
    private readonly IContatoService _contatoService;

    public PropostasController(IPropostaService service, IContatoService contatoService)
    {
        _service = service;
        _contatoService = contatoService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType<PropostaResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<PropostaResponse>> Solicitar([FromBody] SolicitarPropostaRequest request, CancellationToken ct)
    {
        var r = await _service.SolicitarAsync(request, ct);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r);
    }

    [HttpGet]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<PagedResult<ContatoResponse>>> Listar(
        [FromQuery] bool? respondido, [FromQuery] int? unidadeId, [FromQuery] int pagina = 1, [FromQuery] int itensPorPagina = 12, CancellationToken ct = default)
    {
        var query = new ContatoQuery { Tipo = TipoContato.InicioCompra, Respondido = respondido, UnidadeId = unidadeId, Pagina = pagina, ItensPorPagina = itensPorPagina };
        return Ok(await _contatoService.ListarAsync(query, ct));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = PerfisComercial)]
    public async Task<ActionResult<ContatoResponse>> AtualizarStatus(int id, [FromBody] ResponderContatoRequest request, CancellationToken ct)
    {
        var usuario = new UsuarioAtual(
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            User.FindFirstValue(ClaimTypes.Name) ?? "desconhecido");
        var r = await _contatoService.ResponderAsync(id, request, usuario, ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    private ActionResult Erro<T>(Resultado<T> r) => r.Erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new ProblemDetails { Status = 404, Title = r.Mensagem }),
        TipoErro.Conflito => Conflict(new ProblemDetails { Status = 409, Title = r.Mensagem }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = r.Mensagem })
    };
}
