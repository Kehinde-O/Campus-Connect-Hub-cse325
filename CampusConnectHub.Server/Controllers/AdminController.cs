using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusConnectHub.Infrastructure.Data;
using CampusConnectHub.Shared.DTOs;
using System.Globalization;

namespace CampusConnectHub.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors] // Enable CORS for all actions
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

    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsDto>> GetAnalytics()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var sixMonthsAgo = now.AddMonths(-6);

        var totalUsers = await _context.Users.CountAsync();
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth);
        var totalNewsPosts = await _context.NewsPosts.CountAsync();
        var publishedNewsPosts = await _context.NewsPosts.CountAsync(n => n.IsPublished);
        var totalEvents = await _context.Events.CountAsync();
        var upcomingEvents = await _context.Events.CountAsync(e => e.EventDate >= now);
        var totalRSVPs = await _context.EventRSVPs.CountAsync(r => r.Status == "Confirmed");
        var totalResources = await _context.Resources.CountAsync();

        var eventsWithRSVPs = await _context.Events
            .Include(e => e.RSVPs)
            .Where(e => e.RSVPs.Any())
            .ToListAsync();
        var averageRSVPsPerEvent = eventsWithRSVPs.Any() 
            ? eventsWithRSVPs.Average(e => e.RSVPs.Count(r => r.Status == "Confirmed"))
            : 0;

        // User growth trend (last 6 months)
        var userGrowthTrend = new List<MonthlyTrend>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var count = await _context.Users.CountAsync(u => u.CreatedAt >= monthStart && u.CreatedAt < monthEnd);
            userGrowthTrend.Add(new MonthlyTrend
            {
                Month = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Count = count
            });
        }

        // Event trend (last 6 months)
        var eventTrend = new List<MonthlyTrend>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var count = await _context.Events.CountAsync(e => e.CreatedAt >= monthStart && e.CreatedAt < monthEnd);
            eventTrend.Add(new MonthlyTrend
            {
                Month = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Count = count
            });
        }

        // RSVP trend (last 6 months)
        var rsvpTrend = new List<MonthlyTrend>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var count = await _context.EventRSVPs.CountAsync(r => r.RSVPDate >= monthStart && r.RSVPDate < monthEnd && r.Status == "Confirmed");
            rsvpTrend.Add(new MonthlyTrend
            {
                Month = monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                Count = count
            });
        }

        // Most popular events (top 5 by RSVP count)
        var mostPopularEvents = await _context.Events
            .Include(e => e.RSVPs)
            .OrderByDescending(e => e.RSVPs.Count(r => r.Status == "Confirmed"))
            .Take(5)
            .Select(e => new EventPopularityDto
            {
                EventId = e.Id,
                EventTitle = e.Title,
                EventDate = e.EventDate,
                RSVPCount = e.RSVPs.Count(r => r.Status == "Confirmed")
            })
            .ToListAsync();

        return Ok(new AnalyticsDto
        {
            TotalUsers = totalUsers,
            NewUsersThisMonth = newUsersThisMonth,
            TotalNewsPosts = totalNewsPosts,
            PublishedNewsPosts = publishedNewsPosts,
            TotalEvents = totalEvents,
            UpcomingEvents = upcomingEvents,
            TotalRSVPs = totalRSVPs,
            TotalResources = totalResources,
            AverageRSVPsPerEvent = Math.Round(averageRSVPsPerEvent, 2),
            UserGrowthTrend = userGrowthTrend,
            EventTrend = eventTrend,
            RSVPTrend = rsvpTrend,
            MostPopularEvents = mostPopularEvents
        });
    }
}

