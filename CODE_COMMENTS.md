# Code Documentation Standards

This document outlines the code commenting standards used throughout the Campus Connect Hub project.

## Commenting Guidelines

### XML Documentation Comments

All public classes, methods, and properties use XML documentation comments for IntelliSense support:

```csharp
/// <summary>
/// Brief description of what the method/class does.
/// </summary>
/// <param name="parameterName">Description of the parameter</param>
/// <returns>Description of the return value</returns>
```

### Inline Comments

Inline comments are used to explain:
- Complex business logic
- Non-obvious code decisions
- Workarounds for known issues
- Performance optimizations

### Example: Well-Commented Code

```csharp
/// <summary>
/// Authenticates a user and returns a JWT token upon successful login.
/// </summary>
/// <param name="request">Login credentials (email and password)</param>
/// <returns>AuthResponse containing JWT token and user information</returns>
[HttpPost("login")]
public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
{
    // Query database for user with matching email
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == request.Email);

    // Verify password using BCrypt (secure password hashing)
    if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
    {
        return Unauthorized(new { message = "Invalid email or password" });
    }

    // Generate JWT token with user claims
    var token = _jwtService.GenerateToken(user);

    return Ok(new AuthResponse { Token = token, User = user });
}
```

## Code Comment Coverage

### Controllers
- All public methods have XML documentation
- Complex business logic is explained with inline comments
- Error handling is documented

### Services
- Service classes have class-level documentation
- Public methods are fully documented
- Dependencies are explained

### Entities
- Entity classes have summary comments
- Navigation properties are documented
- Complex relationships are explained

### Frontend Components
- Razor components have purpose comments
- Complex UI logic is explained
- Event handlers are documented

## Areas Requiring Comments

1. **Business Logic**: Any non-trivial business rules
2. **Security**: Authentication, authorization, and data protection
3. **Performance**: Optimizations and caching strategies
4. **Dependencies**: External service integrations
5. **Configuration**: Environment-specific settings

## Comment Maintenance

- Update comments when code changes
- Remove outdated comments
- Keep comments concise and relevant
- Use clear, professional language

