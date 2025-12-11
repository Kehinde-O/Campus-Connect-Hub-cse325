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
[EnableCors] // Enable CORS for all actions
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<EventDto>>> GetEvents(
        [FromQuery] bool upcomingOnly = true,
        [FromQuery] string? search = null,
        [FromQuery] string? location = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

        var query = _context.Events
            .Include(e => e.Creator)
            .Include(e => e.RSVPs)
            .AsQueryable();

        if (upcomingOnly)
        {
            query = query.Where(e => e.EventDate >= DateTime.UtcNow);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(e => 
                e.Title.ToLower().Contains(searchLower) || 
                e.Description.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var locationLower = location.ToLower();
            query = query.Where(e => e.Location.ToLower().Contains(locationLower));
        }

        if (startDate.HasValue)
        {
            query = query.Where(e => e.EventDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.EventDate <= endDate.Value);
        }

        var events = await query
            .OrderBy(e => e.EventDate)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                CreatedBy = e.CreatedBy,
                CreatedByName = $"{e.Creator.FirstName} {e.Creator.LastName}",
                CreatedAt = e.CreatedAt,
                MaxAttendees = e.MaxAttendees,
                CurrentAttendees = e.RSVPs.Count(r => r.Status == "Confirmed"),
                IsUserRSVPed = userId.HasValue && e.RSVPs.Any(r => r.UserId == userId.Value && r.Status == "Confirmed")
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

        var eventEntity = await _context.Events
            .Include(e => e.Creator)
            .Include(e => e.RSVPs)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
        {
            return NotFound();
        }

        return Ok(new EventDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            EventDate = eventEntity.EventDate,
            Location = eventEntity.Location,
            CreatedBy = eventEntity.CreatedBy,
            CreatedByName = $"{eventEntity.Creator.FirstName} {eventEntity.Creator.LastName}",
            CreatedAt = eventEntity.CreatedAt,
            MaxAttendees = eventEntity.MaxAttendees,
            CurrentAttendees = eventEntity.RSVPs.Count(r => r.Status == "Confirmed"),
            IsUserRSVPed = userId.HasValue && eventEntity.RSVPs.Any(r => r.UserId == userId.Value && r.Status == "Confirmed")
        });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var eventEntity = new CampusConnectHub.Infrastructure.Entities.Event
        {
            Title = dto.Title,
            Description = dto.Description,
            EventDate = dto.EventDate,
            Location = dto.Location,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            MaxAttendees = dto.MaxAttendees
        };

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync();

        var creator = await _context.Users.FindAsync(userId);

        return CreatedAtAction(nameof(GetEvent), new { id = eventEntity.Id }, new EventDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            EventDate = eventEntity.EventDate,
            Location = eventEntity.Location,
            CreatedBy = eventEntity.CreatedBy,
            CreatedByName = $"{creator!.FirstName} {creator.LastName}",
            CreatedAt = eventEntity.CreatedAt,
            MaxAttendees = eventEntity.MaxAttendees,
            CurrentAttendees = 0,
            IsUserRSVPed = false
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventDto dto)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
        {
            return NotFound();
        }

        eventEntity.Title = dto.Title;
        eventEntity.Description = dto.Description;
        eventEntity.EventDate = dto.EventDate;
        eventEntity.Location = dto.Location;
        eventEntity.MaxAttendees = dto.MaxAttendees;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var eventEntity = await _context.Events.FindAsync(id);
        if (eventEntity == null)
        {
            return NotFound();
        }

        _context.Events.Remove(eventEntity);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/attendees")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<List<EventAttendeeDto>>> GetEventAttendees(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.RSVPs)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
        {
            return NotFound();
        }

        var attendees = eventEntity.RSVPs
            .Where(r => r.Status == "Confirmed")
            .Select(r => new EventAttendeeDto
            {
                RSVPId = r.Id,
                UserId = r.UserId,
                UserName = $"{r.User.FirstName} {r.User.LastName}",
                UserEmail = r.User.Email,
                RSVPDate = r.RSVPDate,
                Status = r.Status
            })
            .OrderBy(a => a.RSVPDate)
            .ToList();

        return Ok(attendees);
    }

    [HttpGet("{id}/attendees/export")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ExportEventAttendees(int id)
    {
        var eventEntity = await _context.Events
            .Include(e => e.RSVPs)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventEntity == null)
        {
            return NotFound();
        }

        var attendees = eventEntity.RSVPs
            .Where(r => r.Status == "Confirmed")
            .Select(r => new
            {
                Name = $"{r.User.FirstName} {r.User.LastName}",
                Email = r.User.Email,
                RSVPDate = r.RSVPDate.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .OrderBy(a => a.RSVPDate)
            .ToList();

        var csv = "Name,Email,RSVP Date\n";
        foreach (var attendee in attendees)
        {
            csv += $"\"{attendee.Name}\",\"{attendee.Email}\",\"{attendee.RSVPDate}\"\n";
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        return File(bytes, "text/csv", $"event-{id}-attendees-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}

