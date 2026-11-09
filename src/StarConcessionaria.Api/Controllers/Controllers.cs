using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarConcessionaria.Application.DTOs;
using StarConcessionaria.Application.Interfaces;
using StarConcessionaria.Domain.Enums;

namespace StarConcessionaria.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
            return Unauthorized(new { message = "E-mail ou senha inválidos." });

        return Ok(response);
    }
}

[ApiController]
[Route("api/v1/unidades")]
[Produces("application/json")]
public class UnidadesController : ControllerBase
{
    private readonly IUnidadeService _unidadeService;

    public UnidadesController(IUnidadeService unidadeService)
    {
        _unidadeService = unidadeService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObterLojas()
    {
        var lojas = await _unidadeService.ObterLojasAsync();
        return Ok(lojas);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterLojaPorId(int id)
    {
        var loja = await _unidadeService.ObterLojaPorIdAsync(id);
        if (loja == null) return NotFound();
        return Ok(loja);
    }

    [HttpPost]
    [Authorize(Roles = PerfilUsuario.Administrador)]
    public async Task<IActionResult> CriarLoja([FromBody] CriarLojaRequest request)
    {
        var novaLoja = await _unidadeService.CriarLojaAsync(request);
        return CreatedAtAction(nameof(ObterLojaPorId), new { id = novaLoja.IdLoj }, novaLoja);
    }
}

[ApiController]
[Route("api/v1/veiculos")]
[Produces("application/json")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;

    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObterCatalogo([FromQuery] FiltroVeiculosRequest filtros)
    {
        var veiculos = await _veiculoService.ObterCatalogoAsync(filtros);
        return Ok(veiculos);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterDetalhes(int id)
    {
        var veiculo = await _veiculoService.ObterDetalhesAsync(id);
        if (veiculo == null) return NotFound();
        return Ok(veiculo);
    }

    [HttpPost]
    [Authorize(Roles = $"{PerfilUsuario.Administrador},{PerfilUsuario.Comercial},{PerfilUsuario.Logistica}")]
    public async Task<IActionResult> CriarVeiculo([FromBody] CriarVeiculoRequest request)
    {
        var veiculo = await _veiculoService.CriarVeiculoAsync(request);
        return CreatedAtAction(nameof(ObterDetalhes), new { id = veiculo.IdVei }, veiculo);
    }

    [HttpPatch("{id:int}/situacao")]
    [Authorize(Roles = $"{PerfilUsuario.Administrador},{PerfilUsuario.Comercial}")]
    public async Task<IActionResult> AtualizarSituacao(int id, [FromBody] AtualizarSituacaoRequest request)
    {
        var sucesso = await _veiculoService.AtualizarSituacaoAsync(id, request.Situacao, request.Justificativa);
        if (!sucesso) return NotFound();
        return Ok(new { message = $"Situação do veículo {id} atualizada com sucesso para {request.Situacao}." });
    }
}

public record AtualizarSituacaoRequest(string Situacao, string? Justificativa);

[ApiController]
[Route("api/v1/faqs")]
[Produces("application/json")]
public class FaqsController : ControllerBase
{
    private readonly IFaqService _faqService;

    public FaqsController(IFaqService faqService)
    {
        _faqService = faqService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObterFaqs()
    {
        var faqs = await _faqService.ObterFaqsAsync();
        return Ok(faqs);
    }
}
