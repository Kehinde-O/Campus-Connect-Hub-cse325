# Campus Connect Hub - Project Submission

## Team Information

**Team**: Team 14  
**Project**: Campus Connect Hub  
**Course**: CSE 325 - .NET Software Development  
**Semester**: Fall 2024

## Submission Checklist

### Required Artifacts

- [x] **GitHub Repository**: [Link to be provided]
- [x] **Trello Board**: [Link to be provided]
- [x] **Deployed Application**: [Link to be provided]
- [x] **Group Video**: [Link to be provided - YouTube]

### Project Links

1. **GitHub Repository**
   - URL: [To be added]
   - Description: Contains all source code, documentation, and deployment configurations

2. **Trello Board**
   - URL: [To be added]
   - Description: Project management board tracking all development tasks and progress

3. **Deployed Application**
   - URL: [To be added]
   - Description: Live application hosted on Microsoft Azure

4. **Group Video Demonstration**
   - URL: [To be added]
   - Duration: 5-7 minutes
   - Format: YouTube video with all team members participating

## Project Overview

Campus Connect Hub is a full-stack .NET 8 application designed to bridge communication between faculty, student organizations, and the student body. The platform centralizes campus updates, enhances student engagement, and provides a single source of truth for campus activities.

## Technical Implementation

### Architecture
- **Frontend**: Blazor WebAssembly (.NET 8)
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Database**: PostgreSQL (Neon PostgreSQL)
- **Hosting**: Microsoft Azure
  - Frontend: Azure Static Web Apps
  - Backend: Azure App Service (Linux)

### Key Features
1. **User Authentication**: JWT-based authentication for Students and Administrators
2. **News Feed**: Centralized dashboard for campus announcements
3. **Event Management**: Calendar interface with RSVP functionality
4. **Resource Directory**: Quick links to essential campus services
5. **Admin Dashboard**: Content management for administrators

### Technology Stack
- .NET 8 SDK
- Entity Framework Core 8.0
- PostgreSQL (via Npgsql)
- JWT Bearer Authentication
- BCrypt for password hashing
- Blazor WebAssembly

## Requirements Compliance

### Project Specifications

- [x] Plan, design, develop, and deploy a .NET Blazor web application
- [x] Application meets goals and specifications for target audience
- [x] Complete, functional, and usable application
- [x] User authentication implemented
- [x] CRUD functionality implemented
- [x] Application tested for quality assurance
- [x] Trello board used for task management
- [x] GitHub repository for source control
- [x] Code comments and user documentation included
- [x] Application deployed to cloud service

### Design Standards

- [x] **Performance**: Optimized for data transfer, minimal network resources
- [x] **Validation**: Valid HTML5 and CSS3 markup
- [x] **Accessibility**: WCAG 2.1 Level AA compliant
- [x] **Usability**: Intuitive design, responsive across all devices
- [x] **Branding**: Consistent color scheme, typography, and layout
- [x] **Navigation**: Clear and intuitive navigation structure

### Error Handling

- [x] Global exception handling middleware
- [x] User-friendly error messages
- [x] Comprehensive error logging
- [x] Graceful error recovery
- [x] Network error handling
- [x] Validation error feedback

### Documentation

- [x] Code comments throughout codebase
- [x] XML documentation for public APIs
- [x] User guide (USER_GUIDE.md)
- [x] Code documentation standards (CODE_COMMENTS.md)
- [x] Accessibility compliance report (ACCESSIBILITY.md)
- [x] Deployment guide (azure-deploy.md)
- [x] README with setup instructions

## Testing

### Test Accounts

**Administrator:**
- Email: `admin@campus.edu`
- Password: `Admin123!`

**Student:**
- Email: `student@campus.edu`
- Password: `Student123!`

### Test Scenarios

1. **Authentication**
   - [x] User registration
   - [x] User login
   - [x] Invalid credentials handling
   - [x] Token expiration handling

2. **News Feed**
   - [x] View paginated news posts
   - [x] Display loading states
   - [x] Handle empty states

3. **Events**
   - [x] View upcoming events
   - [x] RSVP to events
   - [x] Cancel RSVP
   - [x] Handle full events

4. **Resources**
   - [x] View resource directory
   - [x] Open external links
   - [x] Category filtering

5. **Admin Dashboard**
   - [x] View statistics
   - [x] Create news posts
   - [x] Create events
   - [x] Create resources
   - [x] Delete items

## Deployment

### Azure Configuration

- **Frontend**: Azure Static Web Apps
- **Backend**: Azure App Service (Linux)
- **Database**: Neon PostgreSQL (cloud-hosted)

### Environment Variables

All sensitive configuration is managed through Azure App Service configuration settings:
- Connection strings
- JWT secret keys
- CORS origins

## Known Issues and Limitations

1. **Edit Functionality**: Edit modals for admin management are marked as TODO (future enhancement)
2. **Password Reset**: Not implemented (would require email service integration)
3. **Image Uploads**: News posts and events don't support image uploads (future enhancement)

## Future Enhancements

1. Image upload support for news and events
2. Email notifications for new events
3. Event reminders
4. News post categories and filtering
5. User profile management
6. Password reset functionality
7. Search functionality
8. Event calendar view

## Team Contributions

[To be filled in by team members]

## Video Demonstration Outline

1. **Introduction** (30 seconds)
   - Team introduction
   - Project overview

2. **Authentication** (1 minute)
   - User registration
   - Login process
   - Role-based access

3. **Student Features** (2 minutes)
   - News feed navigation
   - Event browsing and RSVP
   - Resource directory

4. **Admin Features** (2 minutes)
   - Dashboard overview
   - Creating news posts
   - Creating events
   - Managing resources

5. **Technical Highlights** (1 minute)
   - Architecture overview
   - Deployment process
   - Key technologies

6. **Conclusion** (30 seconds)
   - Summary
   - Future enhancements

## Peer Evaluation

Each team member will submit a comprehensive peer review evaluating individual contributions and performance.

---

**Submission Date**: [To be filled]  
**Team Members**: [To be filled]  
**Instructor**: [To be filled]

