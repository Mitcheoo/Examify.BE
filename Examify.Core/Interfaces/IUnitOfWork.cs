// Examify.Core/Interfaces/IUnitOfWork.cs
using Examify.Core.Entities;

namespace Examify.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Exercise> Exercises { get; }
    IRepository<User> Users { get; }
    IRepository<Submission> Submissions { get; }
    IRepository<Wallet> Wallets { get; }
    Task<int> SaveChangesAsync();
}