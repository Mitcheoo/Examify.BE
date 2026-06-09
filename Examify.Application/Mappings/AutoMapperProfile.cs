// Examify.Application/Mappings/AutoMapperProfile.cs
using AutoMapper;
using Examify.Core.Entities;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Submissions;

namespace Examify.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // ========== MAPPING CHO EXERCISE ==========
        CreateMap<Exercise, ExerciseDto>()
            .ForMember(dest => dest.SkillName,
                opt => opt.MapFrom(src => GetSkillName(src.Skill)))
            .ForMember(dest => dest.Parts, opt => opt.Ignore())
            .ForMember(dest => dest.ReadingQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.ListeningQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.WritingQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.SpeakingQuestions, opt => opt.Ignore())
            // ✅ THÊM MAPPING CHO 4 PROPERTY MỚI
            .ForMember(dest => dest.ReadingExerciseId, opt => opt.MapFrom(src => src.ReadingExerciseId))
            .ForMember(dest => dest.ListeningExerciseId, opt => opt.MapFrom(src => src.ListeningExerciseId))
            .ForMember(dest => dest.WritingExerciseId, opt => opt.MapFrom(src => src.WritingExerciseId))
            .ForMember(dest => dest.SpeakingExerciseId, opt => opt.MapFrom(src => src.SpeakingExerciseId));

        // Mapping chi tiết cho Exercise
        CreateMap<Exercise, ExerciseDetailDto>()
            .ForMember(dest => dest.SkillName,
                opt => opt.MapFrom(src => GetSkillName(src.Skill)))
            // ✅ THÊM MAPPING CHO 4 PROPERTY
            .ForMember(dest => dest.ReadingExerciseId, opt => opt.MapFrom(src => src.ReadingExerciseId))
            .ForMember(dest => dest.ListeningExerciseId, opt => opt.MapFrom(src => src.ListeningExerciseId))
            .ForMember(dest => dest.WritingExerciseId, opt => opt.MapFrom(src => src.WritingExerciseId))
            .ForMember(dest => dest.SpeakingExerciseId, opt => opt.MapFrom(src => src.SpeakingExerciseId));

        // ========== MAPPING CHO CÂU HỎI ==========
        CreateMap<ReadingQuestion, ReadingQuestionDto>();
        CreateMap<ListeningQuestion, ListeningQuestionDto>();
        CreateMap<WritingQuestion, WritingQuestionDto>();
        CreateMap<SpeakingQuestion, SpeakingQuestionDto>();

        // ========== MAPPING CHO PART ==========
        CreateMap<Part, PartDto>();

        // ========== MAPPING CHO SUBMISSION ==========
        CreateMap<Submission, SubmissionResultDto>()
            .ForMember(dest => dest.ExerciseTitle,
                opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Title : string.Empty));

        CreateMap<Submission, MySubmissionItemDto>()
            .ForMember(dest => dest.ExerciseTitle,
                opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Title : string.Empty));
    }

    // ✅ Method riêng để lấy tên kỹ năng
    private static string GetSkillName(int skill)
    {
        return skill switch
        {
            0 => "Reading",
            1 => "Listening",
            2 => "Writing",
            3 => "Speaking",
            4 => "Full Test",  
            _ => "Unknown"
        };
    }
}