using CreditConsult.DTOs;
using CreditConsult.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreditConsult.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditConsultController : ControllerBase
{
    private readonly ICreditConsultService _service;
    private readonly ILogger<CreditConsultController> _logger;

    public CreditConsultController(
        ICreditConsultService service,
        ILogger<CreditConsultController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo registro de consulta de crédito
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CreditConsultResponseDto>> Create([FromBody] CreditConsultRequestDto requestDto)
    {
        try
        {
            var result = await _service.CreateAsync(requestDto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating credit consult");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Obtém um registro de consulta de crédito por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CreditConsultResponseDto>> GetById(long id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound($"Credit consult with id {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consult: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Obtém todos os registros de consulta de crédito
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditConsultResponseDto>>> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all credit consults");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Obtém registros por número de crédito
    /// </summary>
    [HttpGet("numero-credito/{numeroCredito}")]
    public async Task<ActionResult<IEnumerable<CreditConsultResponseDto>>> GetByNumeroCredito(string numeroCredito)
    {
        try
        {
            var result = await _service.GetByNumeroCreditoAsync(numeroCredito);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consults by numero credito: {NumeroCredito}", numeroCredito);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Obtém registros por número NFSe
    /// </summary>
    [HttpGet("numero-nfse/{numeroNfse}")]
    public async Task<ActionResult<IEnumerable<CreditConsultResponseDto>>> GetByNumeroNfse(string numeroNfse)
    {
        try
        {
            var result = await _service.GetByNumeroNfseAsync(numeroNfse);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consults by numero nfse: {NumeroNfse}", numeroNfse);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Obtém registros por tipo de crédito
    /// </summary>
    [HttpGet("tipo-credito/{tipoCredito}")]
    public async Task<ActionResult<IEnumerable<CreditConsultResponseDto>>> GetByTipoCredito(string tipoCredito)
    {
        try
        {
            var result = await _service.GetByTipoCreditoAsync(tipoCredito);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit consults by tipo credito: {TipoCredito}", tipoCredito);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Atualiza um registro de consulta de crédito
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CreditConsultResponseDto>> Update(
        long id,
        [FromBody] CreditConsultRequestDto requestDto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, requestDto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Credit consult not found: {Id}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating credit consult: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    /// <summary>
    /// Deleta um registro de consulta de crédito
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound($"Credit consult with id {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting credit consult: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}
