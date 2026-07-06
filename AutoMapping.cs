using AutoMapper;
using DotNetApiTemplate.DTOs.Requests.Data;
using DotNetApiTemplate.DTOs.Responses.Data;
using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.Models.EntityLogs;

namespace DotNetApiTemplate
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            #region  --Model--
            CreateMap<Table1Request, Table1>();
            CreateMap<Table1, Table1Response>();
            CreateMap<User, UserResponse>();
            // CreateMap<, >().ReverseMap();
            #endregion

            #region  --ModelLog--
            CreateMap<Table1, Table1Log>().ReverseMap();
            CreateMap<User, UserLog>().ReverseMap();
            CreateMap<Microsoft.AspNetCore.Identity.IdentityUserRole<string>, UserRoleLog>().ReverseMap();
            #endregion
        }
    }
}
