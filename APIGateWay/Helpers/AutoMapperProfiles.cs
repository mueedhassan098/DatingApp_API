using APIGateWay.Dtos;
using APIGateWay.Entities;
using APIGateWay.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace APIGateWay.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<App_User, MemberDto>().ForMember(dest => dest.PhotoUrl,
                opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();

            CreateMap<MemberUpdateDto, App_User>();
            CreateMap<RegisterDto, App_User>();
            CreateMap<Message,MessageDto>()
                .ForMember(MessageDto => MessageDto.SenderPhotoUrl,
                opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(MessageDto => MessageDto.RecipientPhotoUrl,
                opt => opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));
        }
    }
}
