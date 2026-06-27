// 📁 Examify.Application/Mappings/AutoMapperProfile.cs

using AutoMapper;
using Examify.Application.DTOs.Admin;
using Examify.Application.DTOs.Exercises;
using Examify.Application.DTOs.Session;
using Examify.Application.DTOs.Submissions;
using Examify.Core.Entities;

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
            .ForMember(dest => dest.ReadingExerciseId, opt => opt.MapFrom(src => src.ReadingExerciseId))
            .ForMember(dest => dest.ListeningExerciseId, opt => opt.MapFrom(src => src.ListeningExerciseId))
            .ForMember(dest => dest.WritingExerciseId, opt => opt.MapFrom(src => src.WritingExerciseId))
            .ForMember(dest => dest.SpeakingExerciseId, opt => opt.MapFrom(src => src.SpeakingExerciseId))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source ?? "Hệ thống"))
            // ✅ THÊM DÒNG NÀY
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // ✅ Exercise -> ExerciseDetailDto
        CreateMap<Exercise, ExerciseDetailDto>()
            .ForMember(dest => dest.SkillName,
                opt => opt.MapFrom(src => GetSkillName(src.Skill)))
            .ForMember(dest => dest.ReadingExerciseId, opt => opt.MapFrom(src => src.ReadingExerciseId))
            .ForMember(dest => dest.ListeningExerciseId, opt => opt.MapFrom(src => src.ListeningExerciseId))
            .ForMember(dest => dest.WritingExerciseId, opt => opt.MapFrom(src => src.WritingExerciseId))
            .ForMember(dest => dest.SpeakingExerciseId, opt => opt.MapFrom(src => src.SpeakingExerciseId))
            .ForMember(dest => dest.Questions, opt => opt.Ignore())
            .ForMember(dest => dest.WritingQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.SpeakingQuestions, opt => opt.Ignore())
            // ✅ THÊM DÒNG NÀY
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // ✅ Exercise -> ExerciseListDto
        CreateMap<Exercise, ExerciseListDto>()
            .ForMember(dest => dest.SkillName,
                opt => opt.MapFrom(src => GetSkillName(src.Skill)))
            .ForMember(dest => dest.IsCompleted, opt => opt.Ignore())
            .ForMember(dest => dest.LastScore, opt => opt.Ignore())
            // ✅ THÊM DÒNG NÀY
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

        // ========== MAPPING CHO CÂU HỎI ==========
        CreateMap<ReadingQuestion, ReadingQuestionDto>();
        CreateMap<ListeningQuestion, ListeningQuestionDto>();
        CreateMap<WritingQuestion, WritingQuestionDto>();
        CreateMap<SpeakingQuestion, SpeakingQuestionDto>();

        // ========== MAPPING CHO PART ==========
        CreateMap<Part, PartDto>();

        // ========== MAPPING CHO SUBMISSION ==========
        CreateMap<Submission, SubmissionHistoryDto>()
            .ForMember(dest => dest.ExerciseTitle,
                opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Title : string.Empty))
            .ForMember(dest => dest.SkillName,
                opt => opt.MapFrom(src => GetSkillName(src.SkillType)));

        CreateMap<Submission, SubmissionDetailDto>()
            .ForMember(dest => dest.Details, opt => opt.Ignore())
            .ForMember(dest => dest.AiFeedback, opt => opt.Ignore());

        CreateMap<SubmissionDetail, SubmissionAnswerDetailDto>()
            .ForMember(dest => dest.QuestionText, opt => opt.Ignore())
            .ForMember(dest => dest.Explanation, opt => opt.Ignore());

        CreateMap<Submission, MySubmissionItemDto>()
            .ForMember(dest => dest.SubmissionId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
            .ForMember(dest => dest.ExerciseTitle,
                opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Title : string.Empty));

        // ========== MAPPING CHO SESSION ANSWER ==========
        CreateMap<SessionAnswer, SessionAnswerDto>();

        // ========== MAPPING CHO ADMIN ==========
        CreateMap<CreateExerciseDto, Exercise>();
        CreateMap<UpdateExerciseDto, Exercise>();
        CreateMap<CreateFullTestDto, Exercise>();

        // ========== MAPPING CHO CÂU HỎI ADMIN ==========
        CreateMap<CreateReadingQuestionDto, ReadingQuestion>();
        CreateMap<CreateListeningQuestionDto, ListeningQuestion>();
        CreateMap<CreateWritingQuestionDto, WritingQuestion>();
        CreateMap<CreateSpeakingQuestionDto, SpeakingQuestion>();
    }

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