using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Controllers;

// Handles user management endpoints.
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly ProductDbContext _dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(ProductDbContext dbContext, ILogger<UsersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    // Gets a list of all users in the system.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetUsers()
    {
        _logger.LogInformation("Getting all users");
        
        List<UserResponse> allUsers = await _dbContext.Users
            .Select(user => new UserResponse(user.Id, user.FirstName, user.LastName, user.Email))
            .ToListAsync();
            
        return Ok(allUsers);
    }

    // Fetches a single user by their ID.
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        User? existingUser = await _dbContext.Users.FindAsync(id);
        
        // If user not found, return 404 Not Found.
        if (existingUser is null)
        {
            return NotFound();
        }
        
        UserResponse response = new UserResponse(
            existingUser.Id, 
            existingUser.FirstName, 
            existingUser.LastName, 
            existingUser.Email);
            
        return Ok(response);
    }

    // Creates a new user account.
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        // Validation is handled automatically by the framework using the data annotations
        User newUser = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email
        };

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync();

        UserResponse response = new UserResponse(
            newUser.Id, 
            newUser.FirstName, 
            newUser.LastName, 
            newUser.Email);
            
        return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, response);
    }

    // Updates a user's details.
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        User? existingUser = await _dbContext.Users.FindAsync(id);
        
        // Check if the user exists before trying to update
        if (existingUser is null)
        {
            return NotFound();
        }

        // Apply the new values
        existingUser.FirstName = request.FirstName;
        existingUser.LastName = request.LastName;
        existingUser.Email = request.Email;

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    // Removes a user from the database.
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        User? userToDelete = await _dbContext.Users.FindAsync(id);
        
        // Cannot delete a user that doesn't exist
        if (userToDelete is null)
        {
            return NotFound();
        }

        _dbContext.Users.Remove(userToDelete);
        await _dbContext.SaveChangesAsync();
        
        return NoContent();
    }
}
