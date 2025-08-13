using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
// using NPOI.XSSF.UserModel; //XSSF 用來產生Excel 2007檔案（.xlsx）
// using NPOI.SS.UserModel;
// using NPOI.HSSF.UserModel;
using System.Reflection;
using System.Globalization;
using System.Linq.Dynamic.Core;

namespace DotNetApiTemplate
{
    public class PublicMethod
    {
        /// <summary>
        /// 判斷及拼接where
        /// </summary>
        /// <param name="querySearch">搜尋字</param>
        /// <param name="piInfos">物件屬性</param>
        /// <param name="outResult">DB物件</param>
        /// <returns></returns>
        public static IQueryable<T> setWhereStr<T>(string querySearch, PropertyInfo[] piInfos, IQueryable<T> outResult)
        {
            var queryList = new List<string>();
            DateTime dateTime = DateTime.Now;
            string format = "yyyyMMdd";

            foreach (var pi in piInfos)
            {
                if (DateTime.TryParseExact(querySearch, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                {
                    if (Type.Equals(pi.PropertyType, typeof(DateTime?)))
                    {
                        queryList.Add($"({pi.Name} != null || {pi.Name}.Value.Date >= @0 && {pi.Name}.Value.Date <= @0)");
                    }
                }

                if (Type.Equals(pi.PropertyType, typeof(int)) && querySearch.All(char.IsNumber))
                {
                    queryList.Add($"{pi.Name}.ToString().Contains(\"{querySearch}\")");
                }
                if (Type.Equals(pi.PropertyType, typeof(long)) && querySearch.All(char.IsNumber))
                {
                    queryList.Add($"{pi.Name}.ToString().Contains(\"{querySearch}\")");
                }

                Boolean parsedValue;
                if (Boolean.TryParse(querySearch, out parsedValue))
                {
                    if (Type.Equals(pi.PropertyType, typeof(bool)))
                    {
                        queryList.Add($"{pi.Name} == (\"{querySearch}\")");
                    }
                }

                if (Type.Equals(pi.PropertyType, typeof(string)))
                {
                    queryList.Add($"{pi.Name}.Contains(\"{querySearch}\")");
                }
            }
            //若有日期則擺上對應時間
            outResult = outResult.Where(String.Join(" || ", queryList.ToArray()), dateTime);
            return outResult;
        }

    }
}