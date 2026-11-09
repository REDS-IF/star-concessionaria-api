using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Common;
using StarConcessionaria.Application.Veiculos;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/veiculos")]
public class VeiculosController : ControllerBase
{
    private const string PerfisGestao = "Administrador,Comercial,Logistica";

    private readonly IVeiculoService _service;

    public VeiculosController(IVeiculoService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<VeiculoCatalogoResponse>>> Listar([FromQuery] CatalogoQuery filtro, CancellationToken ct)
    {
        var r = await _service.ListarCatalogoAsync(filtro, ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<VeiculoDetalheResponse>> Obter(int id, CancellationToken ct)
    {
        var veiculo = await _service.ObterAsync(id, ct);
        return veiculo is null ? NotFound() : Ok(veiculo);
    }

    [HttpPost]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<VeiculoDetalheResponse>> Criar([FromBody] SalvarVeiculoRequest request, CancellationToken ct)
    {
        var r = await _service.CriarAsync(request, UsuarioLogado(), ct);
        return r.Sucesso ? CreatedAtAction(nameof(Obter), new { id = r.Valor!.Id }, r.Valor) : Erro(r);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<VeiculoDetalheResponse>> Atualizar(int id, [FromBody] SalvarVeiculoRequest request, CancellationToken ct)
    {
        var r = await _service.AtualizarAsync(id, request, UsuarioLogado(), ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    [HttpPatch("{id:int}/situacao")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<VeiculoDetalheResponse>> AlterarSituacao(int id, [FromBody] AlterarSituacaoRequest request, CancellationToken ct)
    {
        var r = await _service.AlterarSituacaoAsync(id, request, UsuarioLogado(), ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    [HttpPost("{id:int}/imagens")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<VeiculoFotoResponse>> AdicionarImagem(int id, [FromBody] AdicionarFotoRequest request, CancellationToken ct)
    {
        var r = await _service.AdicionarFotoAsync(id, request, UsuarioLogado(), ct);
        return r.Sucesso ? Created($"/api/v1/veiculos/{id}", r.Valor) : Erro(r);
    }

    [HttpPatch("{id:int}/imagens/{imagemId:int}/principal")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<VeiculoFotoResponse>> DefinirImagemPrincipal(int id, int imagemId, CancellationToken ct)
    {
        var r = await _service.DefinirFotoPrincipalAsync(id, imagemId, UsuarioLogado(), ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    [HttpDelete("{id:int}/imagens/{imagemId:int}")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<IActionResult> RemoverImagem(int id, int imagemId, CancellationToken ct)
    {
        var r = await _service.RemoverFotoAsync(id, imagemId, UsuarioLogado(), ct);
        return r.Sucesso ? NoContent() : Erro(r);
    }

    [HttpGet("{id:int}/historico")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<IReadOnlyList<VeiculoHistoricoResponse>>> Historico(int id, CancellationToken ct)
    {
        var r = await _service.ObterHistoricoAsync(id, ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    private UsuarioAtual UsuarioLogado() => new(
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
        User.FindFirstValue(ClaimTypes.Name) ?? "desconhecido");

    private ActionResult Erro<T>(Resultado<T> r) => r.Erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new ProblemDetails { Status = 404, Title = r.Mensagem }),
        TipoErro.Conflito => Conflict(new ProblemDetails { Status = 409, Title = r.Mensagem }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = r.Mensagem })
    };
}
