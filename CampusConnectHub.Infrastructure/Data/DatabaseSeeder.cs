using CampusConnectHub.Infrastructure.Entities;
using BCrypt.Net;

namespace CampusConnectHub.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Users.Any())
        {
            return; // Database already seeded
        }

        // Seed Admin User
        var adminUser = new User
        {
            Email = "admin@campus.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = "Administrator",
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        // Seed Sample Student
        var studentUser = new User
        {
            Email = "student@campus.edu",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
            Role = "Student",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(studentUser);

        context.SaveChanges();

        // Seed Sample News Posts
        var newsPosts = new[]
        {
            new NewsPost
            {
                Title = "Welcome to Campus Connect Hub",
                Content = "We're excited to launch the new Campus Connect Hub! This platform will help keep you informed about all campus activities and events.",
                AuthorId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                IsPublished = true
            },
            new NewsPost
            {
                Title = "Spring Semester Registration Open",
                Content = "Registration for the Spring semester is now open. Please visit the student portal to register for classes.",
                AuthorId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                IsPublished = true
            },
            new NewsPost
            {
                Title = "Library Extended Hours",
                Content = "The library will be open extended hours during finals week. Check the library website for specific hours.",
                AuthorId = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                IsPublished = true
            }
        };
        context.NewsPosts.AddRange(newsPosts);

        // Seed Sample Events
        var events = new[]
        {
            new Event
            {
                Title = "Welcome Back Social",
                Description = "Join us for a welcome back social event with food, games, and music!",
                EventDate = DateTime.UtcNow.AddDays(7),
                Location = "Student Center",
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                MaxAttendees = 100
            },
            new Event
            {
                Title = "Career Fair 2024",
                Description = "Meet with employers and explore career opportunities. Bring your resume!",
                EventDate = DateTime.UtcNow.AddDays(14),
                Location = "Convention Center",
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                MaxAttendees = 200
            },
            new Event
            {
                Title = "Study Skills Workshop",
                Description = "Learn effective study techniques and time management strategies.",
                EventDate = DateTime.UtcNow.AddDays(10),
                Location = "Library Room 201",
                CreatedBy = adminUser.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                MaxAttendees = 50
            }
        };
        context.Events.AddRange(events);

        context.SaveChanges();

        // Seed Sample Resources
        var resources = new[]
        {
            new Resource
            {
                Title = "Student Portal",
                Description = "Access your academic records, register for classes, and view your schedule",
                Url = "https://portal.campus.edu",
                Category = "Academic",
                DisplayOrder = 1,
                IsActive = true
            },
            new Resource
            {
                Title = "Library",
                Description = "Search the library catalog, reserve study rooms, and access online resources",
                Url = "https://library.campus.edu",
                Category = "Academic",
                DisplayOrder = 2,
                IsActive = true
            },
            new Resource
            {
                Title = "Campus Maps",
                Description = "Interactive campus maps and building directories",
                Url = "https://maps.campus.edu",
                Category = "Campus Services",
                DisplayOrder = 3,
                IsActive = true
            },
            new Resource
            {
                Title = "IT Help Desk",
                Description = "Get technical support and access IT resources",
                Url = "https://ithelp.campus.edu",
                Category = "Support",
                DisplayOrder = 4,
                IsActive = true
            },
            new Resource
            {
                Title = "Dining Services",
                Description = "View menus, hours, and meal plan information",
                Url = "https://dining.campus.edu",
                Category = "Campus Services",
                DisplayOrder = 5,
                IsActive = true
            }
        };
        context.Resources.AddRange(resources);

        context.SaveChanges();
    }
}

