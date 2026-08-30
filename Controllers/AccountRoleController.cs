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
    public class AccountRoleController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext context = context;

        [HttpPost("create")]
        public async Task<ActionResult> Create(AccountRoleCommand command, CancellationToken token)
        {
            var isExist = await context.AccountRoles.Where(c => c.AccountId == command.AccountId).FirstOrDefaultAsync(token);
            if (isExist != null)
            {
                return BadRequest();
            }
            AccountRole newAccountRole = new()
            {
                AccountId = command.AccountId,
                RoleId = command.RoleId
            };
            context.AccountRoles.Add(newAccountRole);
            await context.SaveChangesAsync(token);
            return Ok();
        }

        [HttpPost("update")]
        public async Task<ActionResult> Update(AccountRoleCommand command, CancellationToken token)
        {
            var accountExists = await context.Accounts
                .AnyAsync(a => a.Id == command.AccountId, token);

            if (!accountExists)
            {
                return BadRequest($"Account with ID {command.AccountId} does not exist.");
            }

            var existingRoles = await context.AccountRoles
                .Where(c => c.AccountId == command.AccountId)
                .ToListAsync(token);

            if (existingRoles.Any(r => r.RoleId == command.RoleId))
            {
                return Ok(); // Already has the requested role
            }

            context.AccountRoles.RemoveRange(existingRoles);
            context.AccountRoles.Add(new AccountRole
            {
                AccountId = command.AccountId,
                RoleId = command.RoleId
            });

            await context.SaveChangesAsync(token);
            return Ok();
        }

        [HttpGet("readbyaccountid")]
        public async Task<ActionResult<List<AccountRoleResponse>>> ReadByAccountId([FromQuery] Guid id, CancellationToken token)
        {
            var result = await context.AccountRoles.Where(c => c.AccountId == id)
                .Select(s => new AccountRoleResponse
                {
                    RoleId = s.RoleId,
                    AccountId = s.AccountId
                }).ToListAsync(token);
            if (result.Count == 0)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("readbyroleid")]
        public async Task<ActionResult<List<AccountRoleResponse>>> ReadByRoleId([FromQuery] Guid id, CancellationToken token)
        {
            var result = await context.AccountRoles.Where(c => c.RoleId == id)
                .Select(s => new AccountRoleResponse
                {
                    RoleId = s.RoleId,
                    AccountId = s.AccountId
                }).ToListAsync(token);
            if (result.Count == 0)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("readall")]
        public async Task<ActionResult<List<AccountRoleResponse>>> ReadAll(CancellationToken token)
        {
            var result = await context.AccountRoles
                .Select(s => new AccountRoleResponse
                {
                    RoleId = s.RoleId,
                    AccountId = s.AccountId
                }).ToListAsync(token);
            if (result.Count == 0)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
