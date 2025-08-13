using AutoMapper;
using  DotNetApiTemplate.Models;
// using  DotNetApiTemplate.ViewModels;

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