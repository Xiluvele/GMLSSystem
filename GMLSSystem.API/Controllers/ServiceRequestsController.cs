using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GMLSSystem.API.Data;
using GMLSSystem.API.Models;
using GMLSSystem.API.Models.Enums;
using GMLSSystem.API.Services;  
using GMLSSystem.Shared.Models;
using Microsoft.AspNetCore.Authorization;

namespace GMLSSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;  
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,  
            ILogger<ServiceRequestsController> logger)
        {
            _context = context;
            _currencyService = currencyService;
            _logger = logger;
        }

        // GET: api/servicerequests
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ServiceRequestDto>>>> GetServiceRequests(
            [FromQuery] int? contractId,
            [FromQuery] string? status)
        {
            try
            {
                var query = _context.ServiceRequests
                    .Include(r => r.Contract)
                    .AsQueryable();

                if (contractId.HasValue)
                    query = query.Where(r => r.ContractId == contractId.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(r => r.Status.ToString() == status);

                var requests = await query
                    .OrderByDescending(r => r.RequestDate)
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

                        ContractNumber = r.Contract.ContractNumber,
                        ClientName = r.Contract.ClientName,
                        Status = r.Status.ToString()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ServiceRequestDto>>
                {
                    Success = true,
                    Data = requests,
                    Message = "Service requests retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service requests");
                return StatusCode(500, new ApiResponse<List<ServiceRequestDto>>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // GET: api/servicerequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ServiceRequestDto>>> GetServiceRequest(int id)
        {
            try
            {
                var request = await _context.ServiceRequests
                    .Include(r => r.Contract)
                    .FirstOrDefaultAsync(r => r.ServiceRequestId == id);

                if (request == null)
                {
                    return NotFound(new ApiResponse<ServiceRequestDto>
                    {
                        Success = false,
                        Message = "Service request not found"
                    });
                }

                var requestDto = new ServiceRequestDto
                {
                    ServiceRequestId = request.ServiceRequestId,
                    ContractId = request.ContractId,
                    RequestNumber = request.RequestNumber,
                    Description = request.Description,
                    CostUSD = request.CostUSD,
                    CostZAR = request.CostZAR,
                    ExchangeRate = request.ExchangeRate,
                    RequestDate = request.RequestDate,
                    Status = request.Status.ToString()
                };

                return Ok(new ApiResponse<ServiceRequestDto>
                {
                    Success = true,
                    Data = requestDto,
                    Message = "Service request retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service request {Id}", id);
                return StatusCode(500, new ApiResponse<ServiceRequestDto>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // POST: api/servicerequests
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ServiceRequestDto>>> CreateServiceRequest([FromBody] CreateServiceRequestDto createDto)
        {
            try
            {
                var contract = await _context.Contracts.FindAsync(createDto.ContractId);
                if (contract == null)
                {
                    return BadRequest(new ApiResponse<ServiceRequestDto>
                    {
                        Success = false,
                        Message = "Contract not found"
                    });
                }

                if (contract.Status != ContractStatus.Active)
                {
                    return BadRequest(new ApiResponse<ServiceRequestDto>
                    {
                        Success = false,
                        Message = "Service requests can only be created for Active contracts"
                    });
                }

                // Get exchange rate from API
                var rate = await _currencyService.GetExchangeRate("USD", "ZAR");

                var request = new ServiceRequest
                {
                    ContractId = createDto.ContractId,
                    Description = createDto.Description,
                    CostUSD = createDto.CostUSD,
                    CostZAR = createDto.CostUSD * rate,
                    ExchangeRate = rate,
                    Status = RequestStatus.Pending,
                    RequestDate = DateTime.UtcNow
                };

                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();

                var requestDto = new ServiceRequestDto
                {
                    ServiceRequestId = request.ServiceRequestId,
                    ContractId = request.ContractId,
                    RequestNumber = request.RequestNumber,
                    Description = request.Description,
                    CostUSD = request.CostUSD,
                    CostZAR = request.CostZAR,
                    ExchangeRate = request.ExchangeRate,
                    RequestDate = request.RequestDate,
                    Status = request.Status.ToString()
                };

                return Ok(new ApiResponse<ServiceRequestDto>
                {
                    Success = true,
                    Data = requestDto,
                    Message = $"Service request created successfully. Rate: 1 USD = {rate:F2} ZAR"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request");
                return StatusCode(500, new ApiResponse<ServiceRequestDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the service request"
                });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateRequestStatus(int id, [FromBody] UpdateServiceRequestStatusDto dto)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);

                if (request == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Service request not found"
                    });
                }

                if (!Enum.TryParse<RequestStatus>(dto.Status, true, out var newStatus))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid status value"
                    });
                }

                request.Status = newStatus;

                if (newStatus == RequestStatus.Completed)
                {
                    request.CompletionDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Request status updated to {dto.Status}",
                    Data = new
                    {
                        id,
                        status = dto.Status
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request status");

                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // GET: api/servicerequests/contract/5
        [HttpGet("contract/{contractId}")]
        public async Task<ActionResult<ApiResponse<List<ServiceRequestDto>>>> GetRequestsByContract(int contractId)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .Where(r => r.ContractId == contractId)
                    .OrderByDescending(r => r.RequestDate)
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
                        Status = r.Status.ToString()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ServiceRequestDto>>
                {
                    Success = true,
                    Data = requests,
                    Message = "Service requests retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting requests for contract {ContractId}", contractId);
                return StatusCode(500, new ApiResponse<List<ServiceRequestDto>>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }
    }
}