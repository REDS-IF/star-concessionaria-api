using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Institucional;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class InstitucionalController : ControllerBase
{
    private readonly IInstitucionalService _service;

    public InstitucionalController(IInstitucionalService service) => _service = service;

    [HttpGet("institucional/formas-pagamento")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<FormaPagamentoDetalheResponse>>> FormasPagamento(CancellationToken ct) =>
        Ok(await _service.ListarFormasPagamentoAsync(ct));

    [HttpGet("faqs")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<FaqResponse>>> Faqs([FromQuery] string? busca, CancellationToken ct) =>
        Ok(await _service.ListarFaqsAsync(busca, ct));
}
