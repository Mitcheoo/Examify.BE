// Examify.Core/Interfaces/IUnitOfWork.cs
using Examify.Core.Entities;

namespace Examify.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Exercise> Exercises { get; }
    IRepository<User> Users { get; }
    IRepository<Submission> Submissions { get; }
    IRepository<Wallet> Wallets { get; }
    IRepository<ReadingQuestion> ReadingQuestions { get; }  // ✅ THÊM DÒNG NÀY
    IRepository<ListeningQuestion> ListeningQuestions { get; }
    IRepository<WritingQuestion> WritingQuestions { get; }
    IRepository<SpeakingQuestion> SpeakingQuestions { get; }
    IRepository<Transaction> Transactions { get; }

    Task<int> SaveChangesAsync();
}