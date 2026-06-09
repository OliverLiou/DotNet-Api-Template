using AutoMapper;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.EntityLogs;
using DotNetApiTemplate.DTOs.ViewModels.Data;

namespace DotNetApiTemplate
{
    public class AutoMapping : Profile
    {
        public AutoMapping()
        {
            #region  --Model--
            CreateMap<Table1Request, Table1>();
            CreateMap<Table1, Table1Response>();
            // CreateMap<, >().ReverseMap();
            #endregion

            #region  --ModelLog--
            CreateMap<Table1, Table1Log>().ReverseMap();
            CreateMap<User, UserLog>().ReverseMap();
            #endregion
        }
    }
}