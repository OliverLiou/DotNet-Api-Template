using AutoMapper;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.EntityLogs;

namespace DotNetApiTemplate
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            #region  --Model--
            // CreateMap<, >().ReverseMap();
            #endregion

            #region  --ModelLog--
            CreateMap<Table1, Table1Log>().ReverseMap();
            CreateMap<User, UserLog>().ReverseMap();
            #endregion
        }
    }
}