using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Pecas;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/pecas")]
public class PecasController : ControllerBase
{
    private const string PerfisLogistica = "Administrador,Logistica";
    private const string PerfisVenda = "Administrador,Comercial,Logistica";

    private readonly IPecaService _service;

    public PecasController(IPecaService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<PecaResponse>>> Listar([FromQuery] PecaQuery filtro, CancellationToken ct) =>
        Ok(await _service.ListarAsync(filtro, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<PecaResponse>> Obter(int id, CancellationToken ct)
    {
        var peca = await _service.ObterAsync(id, ct);
        return peca is null ? NotFound() : Ok(peca);
    }

    [HttpPost]
    [Authorize(Roles = PerfisLogistica)]
    public async Task<ActionResult<PecaResponse>> Criar([FromBody] SalvarPecaRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = PerfisLogistica)]
    public async Task<ActionResult<PecaResponse>> Atualizar(int id, [FromBody] SalvarPecaRequest request, CancellationToken ct)
    {
        var atualizada = await _service.AtualizarAsync(id, request, ct);
        return atualizada is null ? NotFound() : Ok(atualizada);
    }

    [HttpPost("{id:int}/vendas")]
    [Authorize(Roles = PerfisVenda)]
    [ProducesResponseType<VendaPecaResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<VendaPecaResponse>> RegistrarVenda(int id, [FromBody] RegistrarVendaPecaRequest request, CancellationToken ct)
    {
        var r = await _service.RegistrarVendaAsync(id, request, ct);
        if (r.Sucesso) return StatusCode(StatusCodes.Status201Created, r.Valor);

        return r.Erro switch
        {
            TipoErro.NaoEncontrado => NotFound(new ProblemDetails { Status = 404, Title = r.Mensagem }),
            TipoErro.Conflito => Conflict(new ProblemDetails { Status = 409, Title = r.Mensagem }),
            _ => BadRequest(new ProblemDetails { Status = 400, Title = r.Mensagem })
        };
    }
}
