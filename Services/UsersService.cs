using System.Security.Claims;
using Application.Data;
using Application.Models;

namespace Application.Services;

public class UsersService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContext;

    public UsersService
    (
        ApplicationDbContext dbContext,
        IHttpContextAccessor httpContext
    )
    {
        _dbContext = dbContext;
        _httpContext = httpContext;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        try
        {
            var requestingUser = _httpContext.HttpContext.User;

            string currentUserId = requestingUser.FindFirstValue(ClaimTypes.NameIdentifier);

            var currentUser = await _dbContext.Users.FindAsync(currentUserId);

            return currentUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return null;
        }
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _dbContext.Users.FindAsync(userId);

            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

            return null;
        }
    }

}