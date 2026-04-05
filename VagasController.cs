using Microsoft.AspNetCore.Mvc;
using AsylumVagasAPI.DTOs;
using AsylumVagasAPI.Interfaces;

namespace AsylumVagasAPI.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class VagasController : ControllerBase
{
    private readonly IVagaRepository _repository;
    private readonly ILogger<VagasController> _logger;

    public VagasController(IVagaRepository repository, ILogger<VagasController> logger)
    {
        _repository = repository;
        _logger     = logger;
    }

    // ─── GET /api/vagas/disponiveis ───────────────────────────────────────────
    /// <summary>
    /// Retorna todos os asilos com quartos que possuem vagas disponíveis.
    /// As vagas são calculadas dinamicamente: CapacidadeTotal - Residentes Ativos.
    /// </summary>
    [HttpGet("vagas/disponiveis")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AsiloComVagasDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVagasDisponiveis()
    {
        try
        {
            var vagas = await _repository.GetVagasDisponiveisAsync();
            return Ok(ApiResponse<IEnumerable<AsiloComVagasDto>>.Ok(vagas));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar vagas disponíveis");
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao buscar vagas disponíveis."));
        }
    }

    // ─── POST /api/asilos ─────────────────────────────────────────────────────
    /// <summary>Cadastra uma nova instituição asilar.</summary>
    [HttpPost("asilos")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsilo([FromBody] CreateAsiloDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dados inválidos."));

        try
        {
            var asilo = await _repository.CreateAsiloAsync(dto);
            return CreatedAtAction(
                nameof(CreateAsilo),
                new { id = asilo.Id },
                ApiResponse<object>.Ok(asilo, "Asilo cadastrado com sucesso.")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar asilo");
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao cadastrar asilo."));
        }
    }

    // ─── POST /api/residentes ─────────────────────────────────────────────────
    /// <summary>
    /// Adiciona um residente a um quarto.
    /// Valida capacidade disponível antes de inserir.
    /// </summary>
    [HttpPost("residentes")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateResidente([FromBody] CreateResidenteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dados inválidos."));

        try
        {
            var capacidadeDisponivel = await _repository.GetCapacidadeDisponivelAsync(dto.QuartoId);

            if (capacidadeDisponivel == -1)
                return BadRequest(ApiResponse<object>.Fail($"Quarto com ID {dto.QuartoId} não encontrado."));

            if (capacidadeDisponivel == 0)
                return Conflict(ApiResponse<object>.Fail("O quarto está sem vagas disponíveis no momento."));

            var residente = await _repository.CreateResidenteAsync(dto);

            return CreatedAtAction(
                nameof(CreateResidente),
                new { id = residente.Id },
                ApiResponse<object>.Ok(residente, "Residente cadastrado com sucesso.")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar residente");
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao cadastrar residente."));
        }
    }

    // ─── POST /api/solicitacoes ───────────────────────────────────────────────
    /// <summary>Cria uma solicitação de vaga (assistentes sociais / famílias).</summary>
    [HttpPost("solicitacoes")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSolicitacao([FromBody] CreateSolicitacaoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dados inválidos."));

        try
        {
            var solicitacao = await _repository.CreateSolicitacaoAsync(dto);

            return CreatedAtAction(
                nameof(CreateSolicitacao),
                new { id = solicitacao.Id },
                ApiResponse<object>.Ok(solicitacao, "Solicitação registrada com sucesso.")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar solicitação de vaga");
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao criar solicitação."));
        }
    }

    // ─── GET /api/solicitacoes/{asiloId} ──────────────────────────────────────
    /// <summary>Lista as solicitações pendentes de um asilo específico.</summary>
    [HttpGet("solicitacoes/{asiloId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSolicitacoesPendentes([FromRoute] int asiloId)
    {
        if (asiloId <= 0)
            return BadRequest(ApiResponse<object>.Fail("ID do asilo inválido."));

        try
        {
            var solicitacoes = await _repository.GetSolicitacoesPendentesByAsiloAsync(asiloId);
            return Ok(ApiResponse<object>.Ok(solicitacoes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar solicitações do asilo {AsiloId}", asiloId);
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao buscar solicitações."));
        }
    }
// ─── PUT /api/asilos/{id} ─────────────────────────────────────────────────
    /// <summary>Atualiza os dados de uma instituição asilar.</summary>
    [HttpPut("asilos/{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsilo(int id, [FromBody] CreateAsiloDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Dados inválidos."));

        try
        {
            var asilo = await _repository.UpdateAsiloAsync(id, dto);
            return Ok(ApiResponse<object>.Ok(asilo, "Asilo atualizado com sucesso."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar asilo {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao atualizar asilo."));
        }
    }

    // ─── DELETE /api/asilos/{id} ──────────────────────────────────────────────
    /// <summary>Remove uma instituição do sistema.</summary>
    [HttpDelete("asilos/{id:int}")]
    public async Task<IActionResult> DeleteAsilo(int id)
    {
        try
        {
            var removido = await _repository.DeleteAsiloAsync(id);
            if (!removido)
                return NotFound(ApiResponse<object>.Fail("Asilo não encontrado."));

            // CORREÇÃO: Usando new { } em vez de null para evitar warning
            return Ok(ApiResponse<object>.Ok(new { }, "Asilo removido com sucesso."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover asilo {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao remover asilo."));
        }
    }

    [HttpPut("quartos/{id:int}")]
    // CORREÇÃO: Tipando o parâmetro corretamente
    public async Task<IActionResult> UpdateQuarto(int id, [FromBody] UpdateQuartoDto dto)
    {
        try
        {
            var sucesso = await _repository.UpdateQuartoAsync(id, dto);
            if (!sucesso) return NotFound(ApiResponse<object>.Fail("Quarto não encontrado."));
            
            // CORREÇÃO: Usando new { } em vez de null para evitar warning
            return Ok(ApiResponse<object>.Ok(new { }, "Quarto atualizado com sucesso."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar quarto {Id}", id);
            return StatusCode(500, ApiResponse<object>.Fail("Erro interno ao atualizar quarto."));
        }
    }

}
