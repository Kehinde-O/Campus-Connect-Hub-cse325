using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CampusConnectHub.Infrastructure.Data;
using CampusConnectHub.Shared.DTOs;

namespace CampusConnectHub.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserDto>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u => 
                u.Email.ToLower().Contains(searchLower) ||
                u.FirstName.ToLower().Contains(searchLower) ||
                u.LastName.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role == role);
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                NewsPostsCount = u.NewsPosts.Count,
                EventsCreatedCount = u.CreatedEvents.Count,
                RSVPsCount = u.EventRSVPs.Count(r => r.Status == "Confirmed")
            })
            .ToListAsync();

        return Ok(new PagedResponse<UserDto>
        {
            Items = users,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                CreatedAt = u.CreatedAt,
                NewsPostsCount = u.NewsPosts.Count,
                EventsCreatedCount = u.CreatedEvents.Count,
                RSVPsCount = u.EventRSVPs.Count(r => r.Status == "Confirmed")
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Prevent self-demotion if user is the last admin
        if (user.Id == currentUserId && dto.Role != "Administrator")
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == "Administrator");
            if (adminCount <= 1)
            {
                return BadRequest(new { message = "Cannot remove the last administrator" });
            }
        }

        user.Role = dto.Role;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Prevent self-deletion
        if (user.Id == currentUserId)
        {
            return BadRequest(new { message = "Cannot delete your own account" });
        }

        // Prevent deletion of last admin
        if (user.Role == "Administrator")
        {
            var adminCount = await _context.Users.CountAsync(u => u.Role == "Administrator");
            if (adminCount <= 1)
            {
                return BadRequest(new { message = "Cannot delete the last administrator" });
            }
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/activity")]
    public async Task<ActionResult<UserActivityDto>> GetUserActivity(int id)
    {
        var user = await _context.Users
            .Include(u => u.NewsPosts)
            .Include(u => u.CreatedEvents)
            .Include(u => u.EventRSVPs)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        var lastActivity = new List<DateTime>();
        if (user.NewsPosts.Any())
        {
            lastActivity.Add(user.NewsPosts.Max(n => n.CreatedAt));
        }
        if (user.CreatedEvents.Any())
        {
            lastActivity.Add(user.CreatedEvents.Max(e => e.CreatedAt));
        }
        if (user.EventRSVPs.Any())
        {
            lastActivity.Add(user.EventRSVPs.Max(r => r.RSVPDate));
        }

        return Ok(new UserActivityDto
        {
            UserId = user.Id,
            TotalNewsPosts = user.NewsPosts.Count,
            TotalEventsCreated = user.CreatedEvents.Count,
            TotalRSVPs = user.EventRSVPs.Count(r => r.Status == "Confirmed"),
            LastActivityDate = lastActivity.Any() ? lastActivity.Max() : user.CreatedAt
        });
    }
}

