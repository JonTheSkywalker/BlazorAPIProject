using BlazorAPIProject.DataAccess.DataContext;
using BlazorAPIProject.DataAccess.Entities.User;
using BlazorAPIProject.Models.Commands.Users;
using BlazorAPIProject.Models.Responses.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazorAPIProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext context = context;

        [HttpPost("create")]
        public async Task<ActionResult<TokenResponse>> Create(TokenCommand command, CancellationToken token)
        {
            var isExist = await context.Tokens.Where(c => c.Email == command.Email && c.Expiration > DateTime.UtcNow).FirstOrDefaultAsync(token);
            if (isExist != null)
            {
                return BadRequest();
            }

            Token newToken = new()
            {
                Email = command.Email,
                Value = command.Value,
                Expiration = DateTime.Now.AddHours(1)
            };
            context.Tokens.Add(newToken);
            await context.SaveChangesAsync(token);

            return Ok(new TokenResponse { Value = newToken.Value });
        }

        [HttpGet("readbyemail")]
        public async Task<ActionResult> ReadByEmail([FromQuery] string email, [FromQuery] string tokenValue, CancellationToken token)
        {
            var isExist = await context.Tokens
                .Where(c => c.Email == email && c.Value == tokenValue && c.Expiration > DateTime.UtcNow)
                .FirstOrDefaultAsync(token);

            if (isExist == null)
            {
                return NotFound();
            }
            isExist.Expiration = DateTime.UtcNow.AddSeconds(-1);
            context.Tokens.Update(isExist);
            await context.SaveChangesAsync(token);
            return Ok();
        }
    }
}
