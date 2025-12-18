using System.Text.Json.Serialization;

namespace CreditConsult.DTOs;

public class CreditConsultResponseDto
{
    [JsonPropertyName("numeroCredito")]
    public string NumeroCredito { get; set; } = string.Empty;

    [JsonPropertyName("numeroNfse")]
    public string NumeroNfse { get; set; } = string.Empty;

    [JsonPropertyName("dataConstituicao")]
    public DateTime DataConstituicao { get; set; }

    [JsonPropertyName("valorIssqn")]
    public decimal ValorIssqn { get; set; }

    [JsonPropertyName("tipoCredito")]
    public string TipoCredito { get; set; } = string.Empty;

    [JsonPropertyName("simplesNacional")]
    public string SimplesNacional { get; set; } = "Não"; // "Sim" ou "Não" para JSON

    [JsonPropertyName("aliquota")]
    public decimal Aliquota { get; set; }

    [JsonPropertyName("valorFaturado")]
    public decimal ValorFaturado { get; set; }

    [JsonPropertyName("valorDeducao")]
    public decimal ValorDeducao { get; set; }

    [JsonPropertyName("baseCalculo")]
    public decimal BaseCalculo { get; set; }

    // Propriedade interna para conversão (não serializada)
    [JsonIgnore]
    public long Id { get; set; }

    // Propriedade interna para o modelo (não serializada)
    [JsonIgnore]
    internal bool SimplesNacionalBool
    {
        get => SimplesNacional.Equals("Sim", StringComparison.OrdinalIgnoreCase);
        set => SimplesNacional = value ? "Sim" : "Não";
    }
}
