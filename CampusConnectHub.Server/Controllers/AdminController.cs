using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusConnectHub.Infrastructure.Data;
using CampusConnectHub.Shared.DTOs;

namespace CampusConnectHub.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardDto>> GetDashboard()
    {
        var totalNewsPosts = await _context.NewsPosts.CountAsync();
        var publishedNewsPosts = await _context.NewsPosts.CountAsync(n => n.IsPublished);
        var totalEvents = await _context.Events.CountAsync();
        var upcomingEvents = await _context.Events.CountAsync(e => e.EventDate >= DateTime.UtcNow);
        var totalRSVPs = await _context.EventRSVPs.CountAsync(r => r.Status == "Confirmed");
        var totalUsers = await _context.Users.CountAsync();
        var totalResources = await _context.Resources.CountAsync();

        return Ok(new AdminDashboardDto
        {
            TotalNewsPosts = totalNewsPosts,
            PublishedNewsPosts = publishedNewsPosts,
            TotalEvents = totalEvents,
            UpcomingEvents = upcomingEvents,
            TotalRSVPs = totalRSVPs,
            TotalUsers = totalUsers,
            TotalResources = totalResources
        });
    }
}

