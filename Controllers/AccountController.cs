using BlazorAPIProject.DataAccess.DataContext;
using BlazorAPIProject.DataAccess.Entities.User;
using BlazorAPIProject.Models.Commands.Users;
using BlazorAPIProject.Models.Responses.Users;
using BlazorAPIProject.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace BlazorAPIProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(ApplicationDbContext context, HashingHelper hashingHelper, IConfiguration configuration) : Controller
    {
        private readonly ApplicationDbContext context = context;
        private readonly HashingHelper _hashingHelper = hashingHelper;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("create")]
        public async Task<ActionResult<AccountResponse>> Create(AccountCommand command, CancellationToken token)
        {
            var isExist = await context.Accounts.Where(c => c.Email == command.Email).FirstOrDefaultAsync(token);
            if (isExist != null)
            {
                return BadRequest();
            }

            Account newAccount = new()
            {
                Email = command.Email,
                Username = command.Username,
                Password = _hashingHelper.EncryptString(command.Password),
            };
            context.Accounts.Add(newAccount);
            await context.SaveChangesAsync(token);

            var created = await context.Accounts.Where(c => c.Email == newAccount.Email).FirstOrDefaultAsync(token);
            if (created == null)
            {
                return BadRequest();
            }

            return Ok(new AccountResponse
            {
                Id = created.Id,
                Email = created.Email,
                Username = created.Username,
                IsActive = created.IsActive,
            });
        }

        [HttpGet("delete")]
        public async Task<ActionResult> Delete([FromQuery] Guid id, CancellationToken token)
        {
            var isExist = await context.Accounts.Where(c => c.Id == id).FirstOrDefaultAsync(token);
            if (isExist == null)
            {
                return NotFound();
            }
            isExist.IsActive = false;
            context.Accounts.Update(isExist);
            await context.SaveChangesAsync(token);
            return Ok();
        }

        [HttpGet("readall")]
        public async Task<ActionResult<List<AccountResponse>>> ReadAll(CancellationToken token)
        {
            var accounts = await context.Accounts.Where(c => c.IsActive == true)
                .Select(s => new AccountResponse
                {
                    Id = s.Id,
                    Email = s.Email,
                    Username = s.Username,
                    IsActive = s.IsActive,
                }).ToListAsync(token);
            if (accounts.Count == 0)
            {
                return NotFound();
            }
            return Ok(accounts);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginCommand command, CancellationToken token)
        {
            var encryptedPassword = _hashingHelper.EncryptString(command.Password);
            var account = await context.Accounts
                .Where(c => c.Email == command.Email && c.Password == encryptedPassword && c.IsActive)
                .FirstOrDefaultAsync(token);

            if (account == null)
            {
                return Unauthorized();
            }

            var signingKey = _configuration["Authentication:Bearer:SigningKey"];
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                return StatusCode(500, "JWT signing key is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(signingKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accountClaimValue = JsonSerializer.Serialize(new { Id = account.Id, Email = account.Email });

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim("username", account.Username),
                new Claim("Account", accountClaimValue),
            };

            var jwtToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return Ok(new LoginResponse { Token = tokenString });
        }
    }
}
