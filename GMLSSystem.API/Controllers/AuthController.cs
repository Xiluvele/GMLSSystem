using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GMLSSystem.API.Models;
using GMLSSystem.API.Services;
using GMLSSystem.Shared.Models;

namespace GMLSSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IJwtService jwtService,
            ILogger<AuthController> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult<ApiResponse<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (request.Username != "admin@glms.com" ||
                    request.Password != "Admin@123")
                {
                    return Unauthorized(new ApiResponse<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                var user = new ApplicationUser
                {
                    Id = "1",
                    UserName = "admin@glms.com",
                    Email = "admin@glms.com",
                    FullName = "System Administrator",
                    IsActive = true
                };

                var roles = new List<string> { "Admin" };

                var token = _jwtService.GenerateToken(user, roles);

                return Ok(new ApiResponse<LoginResponseDto>
                {
                    Success = true,
                    Data = new LoginResponseDto
                    {
                        Token = token,
                        Username = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        Roles = roles,
                        ClientId = user.ClientId
                    },
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");

                return StatusCode(500, new ApiResponse<LoginResponseDto>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}