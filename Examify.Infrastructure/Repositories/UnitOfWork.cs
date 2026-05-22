// Examify.Infrastructure/Repositories/UnitOfWork.cs
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Data;

namespace Examify.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Exercises = new GenericRepository<Exercise>(context);
        Users = new GenericRepository<User>(context);
        Submissions = new GenericRepository<Submission>(context);
        Wallets = new GenericRepository<Wallet>(context);
    }

    public IRepository<Exercise> Exercises { get; }
    public IRepository<User> Users { get; }
    public IRepository<Submission> Submissions { get; }
    public IRepository<Wallet> Wallets { get; }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}