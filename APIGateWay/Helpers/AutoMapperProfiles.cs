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
            CreateMap<MemberUpdateDto,App_User>();
     //       CreateMap<MemberDto, App_User>()
     //.ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
