using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CampusConnectHub.Infrastructure.Data;
using CampusConnectHub.Shared.DTOs;

namespace CampusConnectHub.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors] // Enable CORS for all actions
public class ResourcesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ResourcesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ResourceDto>>> GetResources()
    {
        var resources = await _context.Resources
            .Where(r => r.IsActive)
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.Title)
            .Select(r => new ResourceDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Url = r.Url,
                Category = r.Category,
                DisplayOrder = r.DisplayOrder,
                IsActive = r.IsActive
            })
            .ToListAsync();

        return Ok(resources);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResourceDto>> GetResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        return Ok(new ResourceDto
        {
            Id = resource.Id,
            Title = resource.Title,
            Description = resource.Description,
            Url = resource.Url,
            Category = resource.Category,
            DisplayOrder = resource.DisplayOrder,
            IsActive = resource.IsActive
        });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceDto dto)
    {
        var resource = new CampusConnectHub.Infrastructure.Entities.Resource
        {
            Title = dto.Title,
            Description = dto.Description,
            Url = dto.Url,
            Category = dto.Category,
            DisplayOrder = dto.DisplayOrder,
            IsActive = dto.IsActive
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, new ResourceDto
        {
            Id = resource.Id,
            Title = resource.Title,
            Description = resource.Description,
            Url = resource.Url,
            Category = resource.Category,
            DisplayOrder = resource.DisplayOrder,
            IsActive = resource.IsActive
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateResource(int id, [FromBody] CreateResourceDto dto)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound();
        }

        resource.Title = dto.Title;
        resource.Description = dto.Description;
        resource.Url = dto.Url;
        resource.Category = dto.Category;
        resource.DisplayOrder = dto.DisplayOrder;
        resource.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null)
        {
            return NotFound();
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

