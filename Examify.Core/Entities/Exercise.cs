// Examify.Core/Entities/Exercise.cs
namespace Examify.Core.Entities;

public class Exercise : BaseEntity
{
    public int Skill { get; set; } // 0: Reading, 1: Listening, 2: Writing, 3: Speaking, 4: Full Test
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AudioUrl { get; set; }
    public string? Passage { get; set; }
    public string? PassagesJson { get; set; }
    public int TotalParts { get; set; } = 3;
    public int TotalQuestions { get; set; }
    public int TimeLimitSeconds { get; set; } = 3600;
    public int Difficulty { get; set; } = 1;
    public bool IsFullTest { get; set; } = false;
    public int AttemptCount { get; set; } = 0;

    public Guid? FullTestId { get; set; }  // Liên kết Full Test

    public string? Source { get; set; } // nguồn đề ở đâu

    // ✅ THÊM 4 PROPERTY NÀY ĐỂ LIÊN KẾT FULL TEST VỚI CÁC BÀI THI KỸ NĂNG
    public Guid? ReadingExerciseId { get; set; }
    public Guid? ListeningExerciseId { get; set; }
    public Guid? WritingExerciseId { get; set; }
    public Guid? SpeakingExerciseId { get; set; }

    // Navigation properties
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
    public virtual ICollection<ReadingQuestion> ReadingQuestions { get; set; } = new List<ReadingQuestion>();
    public virtual ICollection<ListeningQuestion> ListeningQuestions { get; set; } = new List<ListeningQuestion>();
    public virtual ICollection<WritingQuestion> WritingQuestions { get; set; } = new List<WritingQuestion>();
    public virtual ICollection<SpeakingQuestion> SpeakingQuestions { get; set; } = new List<SpeakingQuestion>();
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    // ✅ Navigation properties cho các bài thi con
    public virtual Exercise? ReadingExercise { get; set; }
    public virtual Exercise? ListeningExercise { get; set; }
    public virtual Exercise? WritingExercise { get; set; }
    public virtual Exercise? SpeakingExercise { get; set; }
}