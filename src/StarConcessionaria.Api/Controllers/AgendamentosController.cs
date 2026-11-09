using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Agendamentos;
using StarConcessionaria.Application.Common;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/agendamentos")]
public class AgendamentosController : ControllerBase
{
    private const string PerfisLeitura = "Administrador,Comercial,Logistica";
    private const string PerfisGestao = "Administrador,Comercial";

    private readonly IAgendamentoService _service;

    public AgendamentosController(IAgendamentoService service) => _service = service;

    [HttpGet("horarios-disponiveis")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<string>>> HorariosDisponiveis(
        [FromQuery] int unidadeId, [FromQuery] DateOnly data, [FromQuery] int? veiculoId, CancellationToken ct)
    {
        var r = await _service.ObterHorariosDisponiveisAsync(unidadeId, data, veiculoId, ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    [HttpPost("visita")]
    [AllowAnonymous]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AgendamentoResponse>> AgendarVisita([FromBody] AgendarVisitaRequest request, CancellationToken ct)
    {
        var r = await _service.AgendarVisitaAsync(request, ct);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r);
    }

    [HttpPost("test-drive")]
    [AllowAnonymous]
    [ProducesResponseType<AgendamentoResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AgendamentoResponse>> AgendarTestDrive([FromBody] AgendarTestDriveRequest request, CancellationToken ct)
    {
        var r = await _service.AgendarTestDriveAsync(request, ct);
        return r.Sucesso ? StatusCode(StatusCodes.Status201Created, r.Valor) : Erro(r);
    }

    [HttpGet]
    [Authorize(Roles = PerfisLeitura)]
    public async Task<ActionResult<PagedResult<AgendamentoResponse>>> Listar([FromQuery] AgendamentoQuery filtro, CancellationToken ct) =>
        Ok(await _service.ListarAsync(filtro, ct));

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<AgendamentoResponse>> AtualizarStatus(int id, [FromBody] AtualizarStatusAgendamentoRequest request, CancellationToken ct)
    {
        var r = await _service.AtualizarStatusAsync(id, request, ct);
        return r.Sucesso ? Ok(r.Valor) : Erro(r);
    }

    private ActionResult Erro<T>(Resultado<T> r) => r.Erro switch
    {
        TipoErro.NaoEncontrado => NotFound(new ProblemDetails { Status = 404, Title = r.Mensagem }),
        TipoErro.Conflito => Conflict(new ProblemDetails { Status = 409, Title = r.Mensagem }),
        _ => BadRequest(new ProblemDetails { Status = 400, Title = r.Mensagem })
    };
}
