namespace AsylumVagasAPI.Models;

public class Quarto
{
    public int Id { get; set; }
    public int AsiloId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int CapacidadeTotal { get; set; }
    public string Tipo { get; set; } = string.Empty; // Masculino, Feminino, Misto
    public decimal PrecoBase { get; set; }
}

public class Residente
{
    public int Id { get; set; }
    public int QuartoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public DateTime DataEntrada { get; set; }
    public string Status { get; set; } = "Ativo"; // Ativo, Obito, Alta
}

public class SolicitacaoVaga
{
    public int Id { get; set; }
    public int AsiloId { get; set; }
    public string NomeSolicitante { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendente"; // Pendente, Aprovada, Rejeitada
    public DateTime DataSolicitacao { get; set; }
}
