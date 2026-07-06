using AutoMapper;
using DotNetWebApiMssql.DTOs.Requests.Data;
using DotNetWebApiMssql.DTOs.Responses.Data;
using DotNetWebApiMssql.Models.Entities;
using DotNetWebApiMssql.Models.EntityLogs;

namespace DotNetWebApiMssql
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
