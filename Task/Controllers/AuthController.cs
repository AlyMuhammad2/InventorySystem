using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task_Interview.Data;
using Task_Interview.Models;
using Task_Interview.Services;

namespace Task_Interview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public AuthController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
             // Check 
            var user = _context.Users
                .Include(u => u.Role) 
                .FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
                return Unauthorized("Invalid username or password.");

            var token = _tokenService.GenerateToken(user.Username, user.Role.Name);
            return Ok(new { Token = token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var userExists = _context.Users.Any(u => u.Username == request.Username);
            if (userExists) return BadRequest("User already exists.");
            var user = new User
            {
                Username = request.Username,
                Password = request.Password, 
                RoleId = 2
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return Ok($"User registered successfully\nUsername :{user.Username}");
        }

    }

    public record LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }

    public record RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
       // public string Role { get; set; } = "User";
    }
}
