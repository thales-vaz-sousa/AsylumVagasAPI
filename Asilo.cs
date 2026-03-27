namespace AsylumVagasAPI.Models;

public class Asilo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = "Criciúma";
    public string Telefone { get; set; } = string.Empty;
}
