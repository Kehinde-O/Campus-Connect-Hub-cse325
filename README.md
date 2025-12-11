# Campus Connect Hub

A full-stack .NET 8 application designed to bridge the communication gap between faculty, student organizations, and the student body. This platform centralizes campus updates, enhances student engagement, and provides a single source of truth for campus activities.

## Features

- **User Authentication**: Secure JWT-based authentication for Students and Administrators
- **Campus News Feed**: Centralized dashboard displaying announcements from faculty and departments
- **Event Management**: Calendar interface allowing students to view upcoming events and RSVP
- **Resource Directory**: Curated list of quick links to essential campus services
- **Admin Dashboard**: Restricted view for faculty to post news and manage events
- **Modern UI/UX**: Beautiful, responsive design with glassmorphism effects and smooth animations

## Technology Stack

- **Frontend**: Blazor WebAssembly (.NET 8)
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: PostgreSQL with Entity Framework Core (Neon PostgreSQL)
- **Hosting**: Microsoft Azure
  - Frontend: Azure Static Web Apps
  - Backend: Azure App Service (Linux)

## Project Structure

```
Campus Connect Hub/
├── CampusConnectHub.Server/          # ASP.NET Core Web API
│   ├── Controllers/                  # API Controllers
│   ├── Services/                     # Business logic services
│   └── Program.cs                    # Application entry point
├── CampusConnectHub.Client/          # Blazor WebAssembly
│   ├── Pages/                        # Razor pages
│   ├── Components/                  # Reusable components
│   ├── Services/                     # Client-side services
│   └── Program.cs                    # Client entry point
├── CampusConnectHub.Shared/          # Shared DTOs
│   └── DTOs/                         # Data Transfer Objects
└── CampusConnectHub.Infrastructure/  # Data access layer
    ├── Data/                         # DbContext and migrations
    └── Entities/                    # Database entities
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- PostgreSQL database (Neon PostgreSQL configured)
- Visual Studio 2022 or VS Code

### Setup

1. Clone the repository
2. The connection string is already configured for Neon PostgreSQL in `CampusConnectHub.Server/appsettings.json`

3. Run the database migrations:
   ```bash
   cd CampusConnectHub.Server
   dotnet ef database update
   ```
   Note: The database will be created automatically on first run if using `EnsureCreated()`.

4. Run the backend:
   ```bash
   cd CampusConnectHub.Server
   dotnet run
   ```
   The API will be available at `https://localhost:7126`

5. Run the frontend:
   ```bash
   cd CampusConnectHub.Client
   dotnet run
   ```
   The client will be available at `https://localhost:5001`

### Default Credentials

- **Admin**: admin@campus.edu / Admin123!
- **Student**: student@campus.edu / Student123!

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### News
- `GET /api/news` - Get paginated news posts
- `GET /api/news/{id}` - Get news post by ID
- `POST /api/news` - Create news post (Admin only)
- `PUT /api/news/{id}` - Update news post (Admin only)
- `DELETE /api/news/{id}` - Delete news post (Admin only)

### Events
- `GET /api/events` - Get events (upcoming only by default)
- `GET /api/events/{id}` - Get event by ID
- `POST /api/events` - Create event (Admin only)
- `PUT /api/events/{id}` - Update event (Admin only)
- `DELETE /api/events/{id}` - Delete event (Admin only)

### RSVP
- `POST /api/eventrsvp/{eventId}` - RSVP to event
- `DELETE /api/eventrsvp/{eventId}` - Cancel RSVP
- `GET /api/eventrsvp/my-rsvps` - Get user's RSVPs

### Resources
- `GET /api/resources` - Get all active resources
- `GET /api/resources/{id}` - Get resource by ID
- `POST /api/resources` - Create resource (Admin only)
- `PUT /api/resources/{id}` - Update resource (Admin only)
- `DELETE /api/resources/{id}` - Delete resource (Admin only)

### Admin
- `GET /api/admin/dashboard` - Get dashboard statistics (Admin only)

## Development

### Adding Migrations

```bash
cd CampusConnectHub.Server
dotnet ef migrations add MigrationName --project ../CampusConnectHub.Infrastructure
dotnet ef database update
```

### Testing

The application includes sample data seeded on first run:
- 3 sample news posts
- 3 sample events
- 5 sample resources
- 1 admin user
- 1 student user

## Deployment

### Quick Start (30 minutes)
See [QUICK-START-AZURE.md](QUICK-START-AZURE.md) for a step-by-step guide to deploy to Azure.

### Detailed Guide
See [AZURE-DEPLOYMENT-GUIDE.md](AZURE-DEPLOYMENT-GUIDE.md) for comprehensive Azure deployment instructions including:
- PostgreSQL database setup
- Backend API deployment to App Service
- Frontend deployment to Static Web Apps
- Database initialization and migrations
- Troubleshooting tips

## Documentation

- **[USER_GUIDE.md](USER_GUIDE.md)** - Comprehensive user guide for students and administrators
- **[CODE_COMMENTS.md](CODE_COMMENTS.md)** - Code documentation standards and commenting guidelines
- **[ACCESSIBILITY.md](ACCESSIBILITY.md)** - WCAG 2.1 Level AA compliance report
- **[azure-deploy.md](azure-deploy.md)** - Azure deployment instructions

## Project Requirements Compliance

### Core Requirements
- [x] .NET Blazor web application
- [x] User authentication (JWT-based)
- [x] CRUD operations (News, Events, Resources)
- [x] Cloud deployment (Azure)
- [x] GitHub repository
- [x] Code comments and documentation
- [x] User documentation

### Design Standards
- [x] Performance optimized
- [x] Valid markup and styling
- [x] WCAG 2.1 Level AA accessibility
- [x] Responsive design
- [x] Consistent branding
- [x] Intuitive navigation

### Error Handling
- [x] Global exception handler
- [x] User-friendly error messages
- [x] Comprehensive error logging
- [x] Graceful error recovery

## License

This project is part of a CSE325 course assignment.

## Team

Team 14 - Campus Connect Hub
