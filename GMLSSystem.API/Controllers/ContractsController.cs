using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.API.Data;
using GMLSSystem.API.Models;
using GMLSSystem.API.Models.Enums;
using GMLSSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace GMLSSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(ApplicationDbContext context, ILogger<ContractsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/contracts
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ContractDto>>>> GetContracts(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? status)
        {
            try
            {
                var query = _context.Contracts
                    .Include(c => c.Client)
                    .AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(c => c.StartDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(c => c.EndDate <= endDate.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(c => c.Status.ToString() == status);

                var contracts = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.ContractId,
                        ContractNumber = c.ContractNumber,
                        ClientId = c.ClientId,
                        ClientName = c.Client != null ? c.Client.Name : c.ClientName,
                        Description = c.Description,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString(),
                        ServiceLevel = c.ServiceLevel.ToString(),
                        SignedAgreementPath = c.SignedAgreementPath,
                        OriginalFileName = c.OriginalFileName,
                        CreatedAt = c.CreatedAt,
                        Region = c.Region
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ContractDto>>
                {
                    Success = true,
                    Data = contracts,
                    Message = "Contracts retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contracts");
                return StatusCode(500, new ApiResponse<List<ContractDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving contracts"
                });
            }
        }

        // GET: api/contracts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ContractDto>>> GetContract(int id)
        {
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.Client)
                    .Include(c => c.ServiceRequests)
                    .FirstOrDefaultAsync(c => c.ContractId == id);

                if (contract == null)
                {
                    return NotFound(new ApiResponse<ContractDto>
                    {
                        Success = false,
                        Message = "Contract not found"
                    });
                }

                var contractDto = new ContractDto
                {
                    ContractId = contract.ContractId,
                    ContractNumber = contract.ContractNumber,
                    ClientId = contract.ClientId,
                    ClientName = contract.Client?.Name ?? contract.ClientName,
                    Description = contract.Description,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    Status = contract.Status.ToString(),
                    ServiceLevel = contract.ServiceLevel.ToString(),
                    SignedAgreementPath = contract.SignedAgreementPath,
                    OriginalFileName = contract.OriginalFileName,
                    CreatedAt = contract.CreatedAt,
                    Region = contract.Region,

                        ServiceRequests = contract.ServiceRequests
                    .Select(r => new ServiceRequestDto
                 {
                 ServiceRequestId = r.ServiceRequestId,
                 ContractId = r.ContractId,
                 RequestNumber = r.RequestNumber,
                 Description = r.Description,
                 CostUSD = r.CostUSD,
                 CostZAR = r.CostZAR,
                 ExchangeRate = r.ExchangeRate,
                 RequestDate = r.RequestDate,
                 Status = r.Status.ToString(),
                 ContractNumber = contract.ContractNumber,
                 ClientName = contract.ClientName
                     })
                     .ToList()
                };

                return Ok(new ApiResponse<ContractDto>
                {
                    Success = true,
                    Data = contractDto,
                    Message = "Contract retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contract {Id}", id);
                return StatusCode(500, new ApiResponse<ContractDto>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // GET: api/contracts/active
        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<ContractDto>>>> GetActiveContracts()
        {
            try
            {
                var contracts = await _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.Status == ContractStatus.Active)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.ContractId,
                        ContractNumber = c.ContractNumber,
                        ClientName = c.Client != null ? c.Client.Name : c.ClientName,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString(),
                        ServiceLevel = c.ServiceLevel.ToString()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ContractDto>>
                {
                    Success = true,
                    Data = contracts,
                    Message = "Active contracts retrieved"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active contracts");
                return StatusCode(500, new ApiResponse<List<ContractDto>>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // GET: api/contracts/expiring
        [HttpGet("expiring")]
        public async Task<ActionResult<ApiResponse<List<ContractDto>>>> GetExpiringContracts()
        {
            try
            {
                var expiringThreshold = DateTime.UtcNow.AddDays(30);
                var contracts = await _context.Contracts
                    .Include(c => c.Client)
                    .Where(c => c.Status == ContractStatus.Active && c.EndDate <= expiringThreshold)
                    .OrderBy(c => c.EndDate)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.ContractId,
                        ContractNumber = c.ContractNumber,
                        ClientName = c.Client != null ? c.Client.Name : c.ClientName,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString(),
                        ServiceLevel = c.ServiceLevel.ToString()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ContractDto>>
                {
                    Success = true,
                    Data = contracts,
                    Message = "Expiring contracts retrieved"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expiring contracts");
                return StatusCode(500, new ApiResponse<List<ContractDto>>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // POST: api/contracts
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ContractDto>>> CreateContract([FromBody] CreateContractDto dto)
        {
            try
            {
                // Validate Client exists
                var client = await _context.Clients.FindAsync(dto.ClientId);
                if (client == null)
                {
                    return BadRequest(new ApiResponse<ContractDto>
                    {
                        Success = false,
                        Message = "Client not found"
                    });
                }

                // Parse Status (default to Draft if not provided)
                var status = ContractStatus.Draft;
                if (!string.IsNullOrEmpty(dto.Status))
                {
                    Enum.TryParse<ContractStatus>(dto.Status, true, out status);
                }

                // Parse ServiceLevel (default to Standard if not provided)
                var serviceLevel = ServiceLevel.Standard;
                if (!string.IsNullOrEmpty(dto.ServiceLevel))
                {
                    Enum.TryParse<ServiceLevel>(dto.ServiceLevel, true, out serviceLevel);
                }

                var contract = new Contract
                {
                    ContractNumber = GenerateContractNumber(),
                    ClientId = dto.ClientId,
                    ClientName = client.Name,
                    Description = dto.Description ?? string.Empty,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    Status = status,
                    ServiceLevel = serviceLevel,
                    Region = dto.Region,
                    SignedAgreementPath = dto.SignedAgreementPath,
                    OriginalFileName = dto.OriginalFileName,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Contracts.Add(contract);
                await _context.SaveChangesAsync();

                var contractDto = new ContractDto
                {
                    ContractId = contract.ContractId,
                    ContractNumber = contract.ContractNumber,
                    ClientId = contract.ClientId,
                    ClientName = contract.ClientName,
                    Description = contract.Description,
                    StartDate = contract.StartDate,
                    EndDate = contract.EndDate,
                    Status = contract.Status.ToString(),
                    ServiceLevel = contract.ServiceLevel.ToString(),
                    SignedAgreementPath = contract.SignedAgreementPath,
                    OriginalFileName = contract.OriginalFileName,
                    CreatedAt = contract.CreatedAt,
                    Region = contract.Region
                };

                return Ok(new ApiResponse<ContractDto>
                {
                    Success = true,
                    Data = contractDto,
                    Message = "Contract created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contract");
                return StatusCode(500, new ApiResponse<ContractDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the contract"
                });
            }
        }

        // PATCH: api/contracts/5/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateContractStatus(int id, [FromBody] UpdateContractStatusDto updateDto)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(id);
                if (contract == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Contract not found"
                    });
                }

                if (Enum.TryParse<ContractStatus>(updateDto.Status, true, out var newStatus))
                {
                    contract.Status = newStatus;
                    await _context.SaveChangesAsync();

                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = $"Contract status updated to {updateDto.Status}",
                        Data = new { id, status = updateDto.Status }
                    });
                }

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid status value"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contract status");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // DELETE: api/contracts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteContract(int id)
        {
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.ServiceRequests)
                    .FirstOrDefaultAsync(c => c.ContractId == id);

                if (contract == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Contract not found"
                    });
                }

                if (contract.ServiceRequests != null && contract.ServiceRequests.Any())
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cannot delete contract with existing service requests"
                    });
                }

                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Contract deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contract {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        private string GenerateContractNumber()
        {
            return $"CTR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}