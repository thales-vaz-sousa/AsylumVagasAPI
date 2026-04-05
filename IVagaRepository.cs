using AsylumVagasAPI.DTOs;
using AsylumVagasAPI.Models;

namespace AsylumVagasAPI.Interfaces;

public interface IVagaRepository
{
    // Vagas
    Task<IEnumerable<AsiloComVagasDto>> GetVagasDisponiveisAsync();

    // Asilos
    Task<Asilo> CreateAsiloAsync(CreateAsiloDto dto);

    // Residentes
    Task<Residente>     CreateResidenteAsync(CreateResidenteDto dto);
    Task<int>           GetCapacidadeDisponivelAsync(int quartoId);

    // Solicitações
    Task<SolicitacaoVaga>             CreateSolicitacaoAsync(CreateSolicitacaoDto dto);
    Task<IEnumerable<SolicitacaoVaga>> GetSolicitacoesPendentesByAsiloAsync(int asiloId);

    Task<Asilo> UpdateAsiloAsync(int id, CreateAsiloDto dto);
    Task<bool> DeleteAsiloAsync(int id);
    Task<bool> UpdateQuartoAsync(int id, UpdateQuartoDto dto);
}
