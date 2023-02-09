using AutoMapper;
using FlosonicsSession.DTOs;
using FlosonicsSession.Models;

namespace FlosonicsSession.Profiles
{
    public class SessionProfile: Profile
    {
        
        public SessionProfile()
        {
            CreateMap<AddSessionDto, Session>();
            
            CreateMap<Session, AddSessionDto>().ReverseMap();
            
            CreateMap<UpdateSessionDto, Session>().ReverseMap();

            CreateMap<Session, UpdateSessionDto>();
        }

    }
}
