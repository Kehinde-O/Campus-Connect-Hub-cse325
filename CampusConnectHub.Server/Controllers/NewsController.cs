using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CampusConnectHub.Infrastructure.Data;
using CampusConnectHub.Shared.DTOs;

namespace CampusConnectHub.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NewsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<NewsPostDto>>> GetNews(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool publishedOnly = true)
    {
        var query = _context.NewsPosts
            .Include(n => n.Author)
            .AsQueryable();

        if (publishedOnly)
        {
            query = query.Where(n => n.IsPublished);
        }

        var totalCount = await query.CountAsync();
        var newsPosts = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NewsPostDto
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                AuthorId = n.AuthorId,
                AuthorName = $"{n.Author.FirstName} {n.Author.LastName}",
                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                IsPublished = n.IsPublished
            })
            .ToListAsync();

        return Ok(new PagedResponse<NewsPostDto>
        {
            Items = newsPosts,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NewsPostDto>> GetNewsPost(int id)
    {
        var newsPost = await _context.NewsPosts
            .Include(n => n.Author)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (newsPost == null)
        {
            return NotFound();
        }

        return Ok(new NewsPostDto
        {
            Id = newsPost.Id,
            Title = newsPost.Title,
            Content = newsPost.Content,
            AuthorId = newsPost.AuthorId,
            AuthorName = $"{newsPost.Author.FirstName} {newsPost.Author.LastName}",
            CreatedAt = newsPost.CreatedAt,
            UpdatedAt = newsPost.UpdatedAt,
            IsPublished = newsPost.IsPublished
        });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<NewsPostDto>> CreateNewsPost([FromBody] CreateNewsPostDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var newsPost = new CampusConnectHub.Infrastructure.Entities.NewsPost
        {
            Title = dto.Title,
            Content = dto.Content,
            AuthorId = userId,
            CreatedAt = DateTime.UtcNow,
            IsPublished = dto.IsPublished
        };

        _context.NewsPosts.Add(newsPost);
        await _context.SaveChangesAsync();

        var author = await _context.Users.FindAsync(userId);

        return CreatedAtAction(nameof(GetNewsPost), new { id = newsPost.Id }, new NewsPostDto
        {
            Id = newsPost.Id,
            Title = newsPost.Title,
            Content = newsPost.Content,
            AuthorId = newsPost.AuthorId,
            AuthorName = $"{author!.FirstName} {author.LastName}",
            CreatedAt = newsPost.CreatedAt,
            UpdatedAt = newsPost.UpdatedAt,
            IsPublished = newsPost.IsPublished
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateNewsPost(int id, [FromBody] CreateNewsPostDto dto)
    {
        var newsPost = await _context.NewsPosts.FindAsync(id);
        if (newsPost == null)
        {
            return NotFound();
        }

        newsPost.Title = dto.Title;
        newsPost.Content = dto.Content;
        newsPost.IsPublished = dto.IsPublished;
        newsPost.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteNewsPost(int id)
    {
        var newsPost = await _context.NewsPosts.FindAsync(id);
        if (newsPost == null)
        {
            return NotFound();
        }

        _context.NewsPosts.Remove(newsPost);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

