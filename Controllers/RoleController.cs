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
    public class RoleController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext context = context;

        [HttpPost("create")]
        public async Task<ActionResult> Create(RoleCommand command, CancellationToken token)
        {
            var isExist = await context.Roles.Where(c => c.Name == command.Name).FirstOrDefaultAsync(token);
            if (isExist != null)
            {
                return BadRequest();
            }
            Role newRole = new()
            {
                Name = command.Name
            };
            context.Roles.Add(newRole);
            await context.SaveChangesAsync(token);
            return Ok();
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update(RoleCommand command, CancellationToken token)
        {
            var isExist = await context.Roles.Where(c => c.Id == command.Id).FirstOrDefaultAsync(token);
            if (isExist == null)
            {
                return NotFound();
            }
            isExist.Name = command.Name;
            context.Roles.Update(isExist);
            await context.SaveChangesAsync(token);
            return Ok();
        }

        [HttpGet("readall")]
        public async Task<ActionResult<List<RoleResponse>>> ReadAll(CancellationToken token)
        {
            var roles = await context.Roles
                .Select(c => new RoleResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .ToListAsync(token);
            return Ok(roles);
        }

        [HttpGet("readbyid")]
        public async Task<ActionResult<RoleResponse>> ReadById([FromQuery] Guid id, CancellationToken token)
        {
            var role = await context.Roles.Where(c => c.Id == id)
                .Select(c => new RoleResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .FirstOrDefaultAsync(token);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        [HttpGet("readbyname")]
        public async Task<ActionResult<RoleResponse>> ReadByName([FromQuery] string name, CancellationToken token)
        {
            var role = await context.Roles.Where(c => c.Name == name)
                .Select(c => new RoleResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .FirstOrDefaultAsync(token);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }
    }
}
