using CreditConsult.DTOs;
using CreditConsult.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreditConsult.Controllers;

[ApiController]
[Route("api/creditos")]
public class CreditConsultController : ControllerBase
{
    private readonly ICreditConsultService _service;
    private readonly IRabbitMQPublisherService _publisherService;
    private readonly ILogger<CreditConsultController> _logger;

    public CreditConsultController(
        ICreditConsultService service,
        IRabbitMQPublisherService publisherService,
        ILogger<CreditConsultController> logger)
    {
        _service = service;
        _publisherService = publisherService;
        _logger = logger;
    }

    /// <summary>
    /// Integrar uma lista de créditos constituídos na base de dados
    /// </summary>
    [HttpPost("integrar-credito-constituido")]
    [ProducesResponseType(typeof(IntegrarCreditoResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IntegrarCreditoResponseDto>> IntegrarCreditoConstituido(
        [FromBody] List<CreditConsultIntegrarDto> creditos)
    {
        try
        {
            if (creditos == null || creditos.Count == 0)
            {
                return BadRequest("A lista de créditos não pode estar vazia");
            }

            // Converte para CreditConsultRequestDto e publica mensagens distintas no RabbitMQ
            var requestDtos = creditos.Select(c => c.ToRequestDto()).ToList();

            await _publisherService.PublishMessagesAsync(requestDtos);

            _logger.LogInformation("{Count} créditos publicados na fila RabbitMQ", creditos.Count);

            return Accepted(new IntegrarCreditoResponseDto { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao integrar créditos constituídos");
            return StatusCode(500, new { error = "Erro ao processar a requisição" });
        }
    }

    /// <summary>
    /// Retorna uma lista de créditos constituídos com base no número da NFS-e
    /// </summary>
    [HttpGet("{numeroNfse}")]
    [ProducesResponseType(typeof(List<CreditConsultResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CreditConsultResponseDto>>> GetByNumeroNfse(string numeroNfse)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroNfse))
            {
                return BadRequest("O número da NFS-e é obrigatório");
            }

            var result = await _service.GetByNumeroNfseAsync(numeroNfse);
            var lista = result.ToList();

            if (!lista.Any())
            {
                return NotFound(new List<CreditConsultResponseDto>());
            }

            return Ok(lista);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar créditos por número NFS-e: {NumeroNfse}", numeroNfse);
            return StatusCode(500, new { error = "Erro ao processar a requisição" });
        }
    }

    /// <summary>
    /// Retorna os detalhes de um crédito constituído específico com base no número do crédito constituído
    /// </summary>
    [HttpGet("credito/{numeroCredito}")]
    [ProducesResponseType(typeof(CreditConsultResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreditConsultResponseDto>> GetByNumeroCredito(string numeroCredito)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(numeroCredito))
            {
                return BadRequest("O número do crédito é obrigatório");
            }

            var result = await _service.GetByNumeroCreditoAsync(numeroCredito);
            var lista = result.ToList();

            if (!lista.Any())
            {
                return NotFound();
            }

            // Retorna o primeiro item (ou único item se houver apenas um)
            return Ok(lista.First());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar crédito por número: {NumeroCredito}", numeroCredito);
            return StatusCode(500, new { error = "Erro ao processar a requisição" });
        }
    }
}
