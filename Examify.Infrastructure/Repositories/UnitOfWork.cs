// Examify.Infrastructure/Repositories/UnitOfWork.cs
using Examify.Core.Entities;
using Examify.Core.Interfaces;
using Examify.Infrastructure.Data;

namespace Examify.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed = false;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Exercises = new GenericRepository<Exercise>(context);
        Users = new GenericRepository<User>(context);
        Submissions = new GenericRepository<Submission>(context);
        Wallets = new GenericRepository<Wallet>(context);
        ReadingQuestions = new GenericRepository<ReadingQuestion>(context);
        ListeningQuestions = new GenericRepository<ListeningQuestion>(context);
        WritingQuestions = new GenericRepository<WritingQuestion>(context);
        SpeakingQuestions = new GenericRepository<SpeakingQuestion>(context);
        Transactions = new GenericRepository<Transaction>(context);
    }

    public IRepository<Exercise> Exercises { get; }
    public IRepository<User> Users { get; }
    public IRepository<Submission> Submissions { get; }
    public IRepository<Wallet> Wallets { get; }
    public IRepository<ReadingQuestion> ReadingQuestions { get; }
    public IRepository<ListeningQuestion> ListeningQuestions { get; }
    public IRepository<WritingQuestion> WritingQuestions { get; }
    public IRepository<SpeakingQuestion> SpeakingQuestions { get; }
    public IRepository<Transaction> Transactions { get; }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}