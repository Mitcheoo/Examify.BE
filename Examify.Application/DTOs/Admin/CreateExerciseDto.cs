// Examify.Application/DTOs/Admin/CreateExerciseDto.cs
namespace Examify.Application.DTOs.Admin;

public class CreateExerciseDto
{
    public string Title { get; set; } = string.Empty;
    public int Skill { get; set; } // 0: Reading, 1: Listening, 2: Writing, 3: Speaking, 4: Full Test
    public string? Description { get; set; }
    public string? AudioUrl { get; set; }
    public string? Passage { get; set; }
    public string? PassagesJson { get; set; }
    public int TotalParts { get; set; } = 3;
    public int TotalQuestions { get; set; }
    public int TimeLimitSeconds { get; set; } = 3600;
    public int Difficulty { get; set; } = 1;
    public bool IsFullTest { get; set; } = false;
    public Guid? ReadingExerciseId { get; set; }
    public Guid? ListeningExerciseId { get; set; }
    public Guid? WritingExerciseId { get; set; }
    public Guid? SpeakingExerciseId { get; set; }
}

public class UpdateExerciseDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AudioUrl { get; set; }
    public string? Passage { get; set; }
    public string? PassagesJson { get; set; }
    public int TotalParts { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeLimitSeconds { get; set; }
    public int Difficulty { get; set; }
    public bool IsFullTest { get; set; }
    public Guid? ReadingExerciseId { get; set; }
    public Guid? ListeningExerciseId { get; set; }
    public Guid? WritingExerciseId { get; set; }
    public Guid? SpeakingExerciseId { get; set; }
}

public class CreatePartDto
{
    public int PartNumber { get; set; }
    public string? Title { get; set; }
    public string? Passage { get; set; }
    public string? AudioUrl { get; set; }
}

public class UpdatePartDto
{
    public string? Title { get; set; }
    public string? Passage { get; set; }
    public string? AudioUrl { get; set; }
}

public class CreateReadingQuestionDto
{
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "multiple_choice";
    public string OptionsJson { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
}

public class UpdateReadingQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "multiple_choice";
    public string OptionsJson { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Explanation { get; set; }
}

public class CreateListeningQuestionDto
{
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Explanation { get; set; }
}

public class UpdateListeningQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string? Explanation { get; set; }
}

public class CreateWritingQuestionDto
{
    public int OrderNumber { get; set; }
    public int TaskType { get; set; } // 1: Task 1, 2: Task 2
    public string PromptText { get; set; } = string.Empty;
    public string? SampleImageUrl { get; set; }
    public string? ModelAnswer { get; set; }
    public string? RubricJson { get; set; }
    public int MinWords { get; set; } = 150;
    public int MaxWords { get; set; } = 300;
    public int RecommendedTimeMinutes { get; set; } = 20;
}

public class UpdateWritingQuestionDto
{
    public string PromptText { get; set; } = string.Empty;
    public string? SampleImageUrl { get; set; }
    public string? ModelAnswer { get; set; }
    public string? RubricJson { get; set; }
    public int MinWords { get; set; }
    public int MaxWords { get; set; }
    public int RecommendedTimeMinutes { get; set; }
}

public class CreateSpeakingQuestionDto
{
    public int PartNumber { get; set; }
    public int OrderNumber { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int PreparationTime { get; set; } = 30;
    public int SpeakingTime { get; set; } = 60;
    public string? SampleAnswer { get; set; }
}

public class UpdateSpeakingQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public int PreparationTime { get; set; }
    public int SpeakingTime { get; set; }
    public string? SampleAnswer { get; set; }
}