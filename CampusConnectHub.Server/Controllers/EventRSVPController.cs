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
[Authorize]
public class EventRSVPController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EventRSVPController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("{eventId}")]
    public async Task<ActionResult<RSVPDto>> RSVPToEvent(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check if event exists
        var eventEntity = await _context.Events
            .Include(e => e.RSVPs)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventEntity == null)
        {
            return NotFound(new { message = "Event not found" });
        }

        // Check if already RSVPed
        var existingRSVP = await _context.EventRSVPs
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (existingRSVP != null)
        {
            if (existingRSVP.Status == "Confirmed")
            {
                return BadRequest(new { message = "You have already RSVPed to this event" });
            }
            else
            {
                // Re-activate cancelled RSVP
                existingRSVP.Status = "Confirmed";
                existingRSVP.RSVPDate = DateTime.UtcNow;
            }
        }
        else
        {
            // Check if event is full
            if (eventEntity.MaxAttendees.HasValue)
            {
                var currentAttendees = eventEntity.RSVPs.Count(r => r.Status == "Confirmed");
                if (currentAttendees >= eventEntity.MaxAttendees.Value)
                {
                    return BadRequest(new { message = "Event is full" });
                }
            }

            // Create new RSVP
            existingRSVP = new CampusConnectHub.Infrastructure.Entities.EventRSVP
            {
                EventId = eventId,
                UserId = userId,
                RSVPDate = DateTime.UtcNow,
                Status = "Confirmed"
            };
            _context.EventRSVPs.Add(existingRSVP);
        }

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);

        return Ok(new RSVPDto
        {
            Id = existingRSVP.Id,
            EventId = eventEntity.Id,
            EventTitle = eventEntity.Title,
            EventDate = eventEntity.EventDate,
            UserId = userId,
            UserName = $"{user!.FirstName} {user.LastName}",
            RSVPDate = existingRSVP.RSVPDate,
            Status = existingRSVP.Status
        });
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> CancelRSVP(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rsvp = await _context.EventRSVPs
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (rsvp == null)
        {
            return NotFound(new { message = "RSVP not found" });
        }

        rsvp.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("my-rsvps")]
    public async Task<ActionResult<List<RSVPDto>>> GetMyRSVPs()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var rsvps = await _context.EventRSVPs
            .Include(r => r.Event)
            .Include(r => r.User)
            .Where(r => r.UserId == userId && r.Status == "Confirmed")
            .OrderBy(r => r.Event.EventDate)
            .Select(r => new RSVPDto
            {
                Id = r.Id,
                EventId = r.EventId,
                EventTitle = r.Event.Title,
                EventDate = r.Event.EventDate,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                RSVPDate = r.RSVPDate,
                Status = r.Status
            })
            .ToListAsync();

        return Ok(rsvps);
    }
}

