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
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(ApplicationDbContext context, ILogger<ClientsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ClientDto>>>> GetClients()
        {
            try
            {
                var clients = await _context.Clients
                    .Select(c => new ClientDto
                    {
                        ClientId = c.ClientId,
                        Name = c.Name,
                        ContactEmail = c.ContactEmail,
                        ContactPhone = c.ContactPhone,
                        Address = c.Address,
                        Region = c.Region,
                        CreatedAt = c.CreatedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ClientDto>>
                {
                    Success = true,
                    Data = clients,
                    Message = "Clients retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clients");
                return StatusCode(500, new ApiResponse<List<ClientDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving clients"
                });
            }
        }

        // GET: api/clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ClientDto>>> GetClient(int id)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.ClientId == id);

                if (client == null)
                {
                    return NotFound(new ApiResponse<ClientDto>
                    {
                        Success = false,
                        Message = "Client not found"
                    });
                }

                var clientDto = new ClientDto
                {
                    ClientId = client.ClientId,
                    Name = client.Name,
                    ContactEmail = client.ContactEmail,
                    ContactPhone = client.ContactPhone,
                    Address = client.Address,
                    Region = client.Region,
                    CreatedAt = client.CreatedAt
                };

                return Ok(new ApiResponse<ClientDto>
                {
                    Success = true,
                    Data = clientDto,
                    Message = "Client retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client {Id}", id);
                return StatusCode(500, new ApiResponse<ClientDto>
                {
                    Success = false,
                    Message = "An error occurred"
                });
            }
        }

        // POST: api/clients
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ClientDto>>> CreateClient([FromBody] ClientDto createDto)
        {
            try
            {
                var client = new Client
                {
                    Name = createDto.Name,
                    ContactEmail = createDto.ContactEmail,
                    ContactPhone = createDto.ContactPhone,
                    Address = createDto.Address,
                    Region = createDto.Region,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                createDto.ClientId = client.ClientId;
                createDto.CreatedAt = client.CreatedAt;

                return Ok(new ApiResponse<ClientDto>
                {
                    Success = true,
                    Data = createDto,
                    Message = "Client created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, new ApiResponse<ClientDto>
                {
                    Success = false,
                    Message = "An error occurred while creating the client"
                });
            }
        }

        // PUT: api/clients/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ClientDto>>> UpdateClient(int id, [FromBody] ClientDto updateDto)
        {
            try
            {
                var client = await _context.Clients.FindAsync(id);
                if (client == null)
                {
                    return NotFound(new ApiResponse<ClientDto>
                    {
                        Success = false,
                        Message = "Client not found"
                    });
                }

                client.Name = updateDto.Name;
                client.ContactEmail = updateDto.ContactEmail;
                client.ContactPhone = updateDto.ContactPhone;
                client.Address = updateDto.Address;
                client.Region = updateDto.Region;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<ClientDto>
                {
                    Success = true,
                    Data = updateDto,
                    Message = "Client updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating client {Id}", id);
                return StatusCode(500, new ApiResponse<ClientDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the client"
                });
            }
        }

        // DELETE: api/clients/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteClient(int id)
        {
            try
            {
                var client = await _context.Clients
                    .Include(c => c.Contracts)
                    .FirstOrDefaultAsync(c => c.ClientId == id);

                if (client == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Client not found"
                    });
                }

                if (client.Contracts != null && client.Contracts.Any())
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Cannot delete client with existing contracts"
                    });
                }

                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Client deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {Id}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the client"
                });
            }
        }
    }
}