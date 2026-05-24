// Examify.Core/Interfaces/IUnitOfWork.cs
using Examify.Core.Entities;

namespace Examify.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IRepository<Exercise> Exercises { get; }
    IRepository<User> Users { get; }
    IRepository<ReadingQuestion> ReadingQuestions { get; }
    IRepository<ListeningQuestion> ListeningQuestions { get; }
    IRepository<WritingQuestion> WritingQuestions { get; }
    IRepository<SpeakingQuestion> SpeakingQuestions { get; }
    IRepository<Submission> Submissions { get; }
    IRepository<SubmissionDetail> SubmissionDetails { get; }
    IRepository<Wallet> Wallets { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<Bookmark> Bookmarks { get; }
    IRepository<Leaderboard> Leaderboards { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<FullTestSession> FullTestSessions { get; }
    IRepository<Part> Parts { get; }

    Task<int> SaveChangesAsync();
}