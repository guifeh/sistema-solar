using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;

namespace SolarSystem.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SolarDbContext _context;

    public UserRepository(SolarDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }
}
