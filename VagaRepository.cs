using System.Data;
using Dapper;
using Npgsql;
using AsylumVagasAPI.DTOs;
using AsylumVagasAPI.Interfaces;
using AsylumVagasAPI.Models;

namespace AsylumVagasAPI.Repositories;

public class VagaRepository : IVagaRepository
{
    private readonly string _connectionString;

    public VagaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    // ─── VAGAS DISPONÍVEIS ────────────────────────────────────────────────────
    // Regra de ouro: vagas calculadas dinamicamente via SQL.
    // Não existe campo editável de vagas livres.
    public async Task<IEnumerable<AsiloComVagasDto>> GetVagasDisponiveisAsync()
    {
        const string sql = """
            SELECT
                a.id            AS AsiloId,
                a.nome          AS Nome,
                a.endereco      AS Endereco,
                a.cidade        AS Cidade,
                a.telefone      AS Telefone,
                q.id            AS QuartoId,
                q.numero        AS Numero,
                q.tipo          AS Tipo,
                q.preco_base    AS PrecoBase,
                q.capacidade_total AS CapacidadeTotal,
                COUNT(r.id) FILTER (WHERE r.status = 'Ativo') AS ResidentesAtivos,
                q.capacidade_total - COUNT(r.id) FILTER (WHERE r.status = 'Ativo') AS VagasDisponiveis
            FROM asilos a
            INNER JOIN quartos q ON q.asilo_id = a.id
            LEFT JOIN residentes r ON r.quarto_id = q.id
            GROUP BY a.id, a.nome, a.endereco, a.cidade, a.telefone,
                     q.id, q.numero, q.tipo, q.preco_base, q.capacidade_total
            HAVING (q.capacidade_total - COUNT(r.id) FILTER (WHERE r.status = 'Ativo')) > 0
            ORDER BY a.nome, q.numero;
            """;

        using var connection = CreateConnection();

        // 1. Buscamos todos os dados como uma lista flat (plana)
        // Usamos dynamic aqui apenas para facilitar a leitura das colunas do SQL
        var rows = await connection.QueryAsync<dynamic>(sql);

        // 2. Agrupamos os dados por Asilo usando LINQ (muito mais limpo e evita o erro CS8917)
        var resultado = rows.GroupBy(r => new { 
            r.asiloid, r.nome, r.endereco, r.cidade, r.telefone 
        })
        .Select(g => new AsiloComVagasDto(
            AsiloId: (int)g.Key.asiloid,
            Nome: (string)g.Key.nome,
            Endereco: (string)g.Key.endereco,
            Cidade: (string)g.Key.cidade,
            Telefone: (string)g.Key.telefone,
            Quartos: g.Select(q => new QuartoDisponivelDto(
                QuartoId: (int)q.quartoid,
                Numero: (string)q.numero,
                Tipo: (string)q.tipo,
                PrecoBase: (decimal)q.precobase,
                CapacidadeTotal: (int)q.capacidadetotal,
                ResidentesAtivos: (int)(long)q.residentesativos,
                VagasDisponiveis: (int)(long)q.vagasdisponiveis
            )).ToList()
        ));

        return resultado;
    }
    // ─── ASILOS ───────────────────────────────────────────────────────────────

    public async Task<Asilo> CreateAsiloAsync(CreateAsiloDto dto)
    {
        const string sql = """
            INSERT INTO asilos (nome, cnpj, endereco, cidade, telefone)
            VALUES (@Nome, @CNPJ, @Endereco, @Cidade, @Telefone)
            RETURNING id, nome, cnpj, endereco, cidade, telefone;
            """;

        using var connection = CreateConnection();

        var asilo = await connection.QuerySingleAsync<Asilo>(sql, new
        {
            dto.Nome,
            dto.CNPJ,
            dto.Endereco,
            Cidade   = dto.Cidade ?? "Criciúma",
            dto.Telefone
        });

        return asilo;
    }

    // ─── RESIDENTES ───────────────────────────────────────────────────────────

    public async Task<int> GetCapacidadeDisponivelAsync(int quartoId)
    {
        const string sql = """
            SELECT
                q.capacidade_total - COUNT(r.id) FILTER (WHERE r.status = 'Ativo') AS vagas_livres
            FROM quartos q
            LEFT JOIN residentes r ON r.quarto_id = q.id
            WHERE q.id = @QuartoId
            GROUP BY q.capacidade_total;
            """;

        using var connection = CreateConnection();

        var vagasLivres = await connection.QuerySingleOrDefaultAsync<long?>(sql, new { QuartoId = quartoId });

        // Retorna -1 se o quarto não existir, sinalizando que o quarto é inválido
        return vagasLivres.HasValue ? (int)vagasLivres.Value : -1;
    }

    public async Task<Residente> CreateResidenteAsync(CreateResidenteDto dto)
    {
        const string sql = """
            INSERT INTO residentes (quarto_id, nome, cpf, data_entrada, status)
            VALUES (@QuartoId, @Nome, @CPF, NOW(), 'Ativo')
            RETURNING id, quarto_id, nome, cpf, data_entrada, status;
            """;

        using var connection = CreateConnection();

        var residente = await connection.QuerySingleAsync<Residente>(sql, new
        {
            dto.QuartoId,
            dto.Nome,
            dto.CPF
        });

        return residente;
    }

    // ─── SOLICITAÇÕES ─────────────────────────────────────────────────────────

    public async Task<SolicitacaoVaga> CreateSolicitacaoAsync(CreateSolicitacaoDto dto)
    {
        const string sql = """
            INSERT INTO solicitacoes_vaga (asilo_id, nome_solicitante, telefone, status, data_solicitacao)
            VALUES (@AsiloId, @NomeSolicitante, @Telefone, 'Pendente', NOW())
            RETURNING id, asilo_id, nome_solicitante, telefone, status, data_solicitacao;
            """;

        using var connection = CreateConnection();

        var solicitacao = await connection.QuerySingleAsync<SolicitacaoVaga>(sql, new
        {
            dto.AsiloId,
            dto.NomeSolicitante,
            dto.Telefone
        });

        return solicitacao;
    }

    public async Task<IEnumerable<SolicitacaoVaga>> GetSolicitacoesPendentesByAsiloAsync(int asiloId)
    {
        const string sql = """
            SELECT id, asilo_id, nome_solicitante, telefone, status, data_solicitacao
            FROM solicitacoes_vaga
            WHERE asilo_id = @AsiloId
              AND status = 'Pendente'
            ORDER BY data_solicitacao ASC;
            """;

        using var connection = CreateConnection();

        return await connection.QueryAsync<SolicitacaoVaga>(sql, new { AsiloId = asiloId });
    }
}
