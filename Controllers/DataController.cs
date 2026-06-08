using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DotNetApiTemplate.DTOs.Entities;
using DotNetApiTemplate.DTOs.EntityLogs;
using DotNetApiTemplate.DTOs.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DotNetApiTemplate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DataController(IRepositoryService<Table1, Table1Log> repositoryService) : ControllerBase
    {
        private readonly IRepositoryService<Table1, Table1Log> _repositoryService = repositoryService;

        [HttpGet("GetTable1/{table1Id}")]
        [SwaggerOperation(Summary = "取得指定的 Table1 資料", Description = "取得指定的 Table1 資料")]
        public async Task<IActionResult> GetTable1(int table1Id)
        {
            var result = await _repositoryService.GetDataWithIdAsync([table1Id]);

            return Ok(result);
        }

        [HttpPost("Table1Save")]
        [SwaggerOperation(Summary = "儲存單筆 Table1 資料", Description = "儲存單筆 Table1 資料")]
        public async Task<IActionResult> Table1Save(Table1 table1)
        {
            var editorName = GetCurrentUserNameFromToken();
            await _repositoryService.SaveSingleDataAsync(table1, editorName);
            return Ok();
        }

        [HttpPost("Table1MutipleSave")]
        [SwaggerOperation(Summary = "儲存多筆 Table1 資料", Description = "儲存多筆 Table1 資料")]
        public async Task<IActionResult> Table1MutipleSave(List<Table1> table1s)
        {
            var editorName = GetCurrentUserNameFromToken();
            await _repositoryService.SaveMultipleDataAsync(table1s, editorName);
            return Ok();
        }

        [HttpPost("RemoveTable1Data")]
        [SwaggerOperation(Summary = "刪除指定的 Table1 資料", Description = "刪除指定的 Table1 資料")]
        public async Task<IActionResult> RemoveTable1Data(int table1Id)
        {
            var editorName = GetCurrentUserNameFromToken();
            await _repositoryService.DeleteSingleDataAsync([table1Id], editorName);

            return Ok();
        }

        [HttpGet("GetTable1s")]
        [SwaggerOperation(Summary = "取得所有 Table1 資料", Description = "取得所有 Table1 資料")]
        public async Task<IActionResult> GetTable1s()
        {
            var allData = await _repositoryService.GetAllDataAsync();
            return Ok(allData);
        }

        [HttpGet("FindTable1/{currentPage}/{pageSize}")]
        [SwaggerOperation(Summary = "分頁查詢 Table1 資料", Description = "分頁查詢 Table1 資料")]
        public async Task<IActionResult> FindTable1(int currentPage, int pageSize, string? querySearch)
        {
            // Expression<Func<Table1, bool>> filter = t => t.Column1 == "xxxw666";
            var sortColumns = new List<(string PropertyName, bool IsAscending)> { ("Table1Id", true) };

            var tuple = await _repositoryService.FindDataAsync(currentPage, pageSize, querySearch, null, sortColumns);

            return Ok(tuple);
        }
    
        /// <summary>
        /// 取得當前JWT Token的使用者名稱，供 RepositoryService 的 Log 紀錄使用
        /// </summary>
        private string GetCurrentUserNameFromToken()
        {
            var nameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name) ?? throw new InvalidOperationException("JWT Token does not contain Name Claim");
            return nameClaim.Value;
        }
    }
}