using System.Text.Json.Serialization;

namespace CreditConsult.DTOs;

/// <summary>
/// DTO para integração de crédito (aceita simplesNacional como string "Sim"/"Não")
/// </summary>
public class CreditConsultIntegrarDto
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
    public string SimplesNacional { get; set; } = string.Empty; // "Sim" ou "Não"

    [JsonPropertyName("aliquota")]
    public decimal Aliquota { get; set; }

    [JsonPropertyName("valorFaturado")]
    public decimal ValorFaturado { get; set; }

    [JsonPropertyName("valorDeducao")]
    public decimal ValorDeducao { get; set; }

    [JsonPropertyName("baseCalculo")]
    public decimal BaseCalculo { get; set; }

    /// <summary>
    /// Converte para CreditConsultRequestDto (com bool para SimplesNacional)
    /// </summary>
    public CreditConsultRequestDto ToRequestDto()
    {
        return new CreditConsultRequestDto
        {
            NumeroCredito = this.NumeroCredito,
            NumeroNfse = this.NumeroNfse,
            DataConstituicao = this.DataConstituicao,
            ValorIssqn = this.ValorIssqn,
            TipoCredito = this.TipoCredito,
            SimplesNacional = this.SimplesNacional.Equals("Sim", StringComparison.OrdinalIgnoreCase),
            Aliquota = this.Aliquota,
            ValorFaturado = this.ValorFaturado,
            ValorDeducao = this.ValorDeducao,
            BaseCalculo = this.BaseCalculo
        };
    }
}

