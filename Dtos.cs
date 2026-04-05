using System.ComponentModel.DataAnnotations;

namespace AsylumVagasAPI.DTOs;

// ─── ASILO ────────────────────────────────────────────────────────────────────

public record CreateAsiloDto(
    [Required, MaxLength(200)] string Nome,
    [Required, MaxLength(18)]  string CNPJ,
    [Required, MaxLength(300)] string Endereco,
    [MaxLength(100)]           string? Cidade,
    [Required, MaxLength(20)]  string Telefone
);

// ─── QUARTO (NOVO - Para resolver o erro do Repository) ───────────────────────

public record UpdateQuartoDto(
    [Required, MaxLength(50)]  string Numero,
    [Required, MaxLength(100)] string Tipo,
    [Required]                 decimal PrecoBase,
    [Required]                 int CapacidadeTotal
);

// ─── RESIDENTE ────────────────────────────────────────────────────────────────

public record CreateResidenteDto(
    [Required]                 int    QuartoId,
    [Required, MaxLength(200)] string Nome,
    [Required, MaxLength(14)]  string CPF
);

// ─── SOLICITAÇÃO ──────────────────────────────────────────────────────────────

public record CreateSolicitacaoDto(
    [Required]                 int    AsiloId,
    [Required, MaxLength(200)] string NomeSolicitante,
    [Required, MaxLength(20)]  string Telefone
);

// ─── VAGAS DISPONÍVEIS ────────────────────────────────────────────────────────

public record QuartoDisponivelDto(
    int     QuartoId,
    string  Numero,
    string  Tipo,
    decimal PrecoBase,
    int     CapacidadeTotal,
    int     ResidentesAtivos,
    int     VagasDisponiveis
);

public record AsiloComVagasDto(
    int    AsiloId, // Garantindo o tipo explícito
    string Nome,
    string Endereco,
    string Cidade,
    string Telefone,
    IEnumerable<QuartoDisponivelDto> Quartos
);

// ─── RESPOSTA PADRÃO ─────────────────────────────────────────────────────────

public record ApiResponse<T>(bool Success, string? Message, T? Data)
{
    public static ApiResponse<T> Ok(T data, string? message = null)
        => new(true, message, data);

    public static ApiResponse<T> Fail(string message)
        => new(false, message, default);
}