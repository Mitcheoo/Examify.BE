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
        // ✅ THÊM MAPPING NÀY
        CreateMap<Exercise, ExerciseDto>()
            .ForMember(dest => dest.SkillName, opt => opt.MapFrom(src => src.Skill.ToString()))
            .ForMember(dest => dest.ReadingQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.ListeningQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.WritingQuestions, opt => opt.Ignore())
            .ForMember(dest => dest.SpeakingQuestions, opt => opt.Ignore());

        // Mapping cho các entity khác
        CreateMap<ReadingQuestion, ReadingQuestionDto>();
        CreateMap<ListeningQuestion, ListeningQuestionDto>();
        CreateMap<WritingQuestion, WritingQuestionDto>();
        CreateMap<SpeakingQuestion, SpeakingQuestionDto>();
        CreateMap<Part, PartDto>();
        CreateMap<Submission, SubmissionResultDto>()
            .ForMember(dest => dest.ExerciseTitle, opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Title : string.Empty));

        CreateMap<Submission, MySubmissionItemDto>()
            .ForMember(dest => dest.ExerciseTitle, opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Title : string.Empty));

        // Examify.Application/Mappings/AutoMapperProfile.cs
        CreateMap<SpeakingQuestion, SpeakingQuestionDto>();
        CreateMap<Part, PartDto>();
    }
}