using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Dashboard;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private const string PerfisGestao = "Administrador,Comercial,Logistica";
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("indicadores")]
    [Authorize(Roles = PerfisGestao)]
    public async Task<ActionResult<IndicadoresGerenciaisResponse>> Indicadores(
        [FromQuery] int? unidadeId, CancellationToken ct) =>
        Ok(await _service.ObterIndicadoresAsync(unidadeId, ct));
}
