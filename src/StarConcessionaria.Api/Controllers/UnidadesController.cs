using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.Unidades;
using StarConcessionaria.Domain.Entities;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/unidades")]
public class UnidadesController : ControllerBase
{
    private readonly IUnidadeService _service;

    public UnidadesController(IUnidadeService service) => _service = service;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<UnidadeResponse>>> Listar(
        [FromQuery] string? cidade, [FromQuery] string? estado, CancellationToken ct) =>
        Ok(await _service.ListarAsync(cidade, estado, ct));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<UnidadeResponse>> Obter(int id, CancellationToken ct)
    {
        var unidade = await _service.ObterAsync(id, ct);
        return unidade is null ? NotFound() : Ok(unidade);
    }

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    public async Task<ActionResult<UnidadeResponse>> Criar([FromBody] SalvarUnidadeRequest request, CancellationToken ct)
    {
        var criada = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = criada.Id }, criada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    public async Task<ActionResult<UnidadeResponse>> Atualizar(int id, [FromBody] SalvarUnidadeRequest request, CancellationToken ct)
    {
        var atualizada = await _service.AtualizarAsync(id, request, ct);
        return atualizada is null ? NotFound() : Ok(atualizada);
    }
}
