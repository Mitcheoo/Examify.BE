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

        // Initialize all repositories
        Exercises = new GenericRepository<Exercise>(context);
        Users = new GenericRepository<User>(context);
        ReadingQuestions = new GenericRepository<ReadingQuestion>(context);
        ListeningQuestions = new GenericRepository<ListeningQuestion>(context);
        WritingQuestions = new GenericRepository<WritingQuestion>(context);
        SpeakingQuestions = new GenericRepository<SpeakingQuestion>(context);
        Submissions = new GenericRepository<Submission>(context);
        SubmissionDetails = new GenericRepository<SubmissionDetail>(context);
        Wallets = new GenericRepository<Wallet>(context);
        Transactions = new GenericRepository<Transaction>(context);
        Bookmarks = new GenericRepository<Bookmark>(context);
        Leaderboards = new GenericRepository<Leaderboard>(context);
        Notifications = new GenericRepository<Notification>(context);
        FullTestSessions = new GenericRepository<FullTestSession>(context);
        Parts = new GenericRepository<Part>(context);
    }

    // Repository Properties
    public IRepository<Exercise> Exercises { get; }
    public IRepository<User> Users { get; }
    public IRepository<ReadingQuestion> ReadingQuestions { get; }
    public IRepository<ListeningQuestion> ListeningQuestions { get; }
    public IRepository<WritingQuestion> WritingQuestions { get; }
    public IRepository<SpeakingQuestion> SpeakingQuestions { get; }
    public IRepository<Submission> Submissions { get; }
    public IRepository<SubmissionDetail> SubmissionDetails { get; }
    public IRepository<Wallet> Wallets { get; }
    public IRepository<Transaction> Transactions { get; }
    public IRepository<Bookmark> Bookmarks { get; }
    public IRepository<Leaderboard> Leaderboards { get; }
    public IRepository<Notification> Notifications { get; }
    public IRepository<FullTestSession> FullTestSessions { get; }
    public IRepository<Part> Parts { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // Dispose pattern
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