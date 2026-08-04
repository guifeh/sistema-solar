using Microsoft.EntityFrameworkCore;
using SolarSystem.Application.Common.Interfaces;
using SolarSystem.Domain.Identity;

namespace SolarSystem.Infrastructure.Persistence.Repositories;

/// <summary>
/// As buscas por e-mail e por id rodam no fluxo de autenticacao, antes de existir um tenant
/// no contexto — por isso ignoram o filtro global. E o unico repositorio autorizado a fazer
/// isso: qualquer consulta de dados de negocio deve respeitar o filtro.
/// </summary>
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
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }
}
