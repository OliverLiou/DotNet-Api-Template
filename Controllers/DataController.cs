using System;
using System.Collections.Generic;
using AutoMapper;
using DotNetApiTemplate.DTOs.Requests.Data;
using DotNetApiTemplate.DTOs.Responses.Data;
using DotNetApiTemplate.Interfaces;
using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.Models.EntityLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace DotNetApiTemplate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DataController(IRepositoryService<Table1, Table1Log> table1RepositoryService, IMapper mapper) : ControllerBase
    {
        private readonly IRepositoryService<Table1, Table1Log> _table1RepositoryService = table1RepositoryService;
        private readonly IMapper _mapper = mapper;

        [HttpGet("GetTable1/{table1Id}")]
        [SwaggerOperation(Description = "取得指定的 Table1 資料")]
        public async Task<IActionResult> GetTable1(int table1Id)
        {
            var result = await _table1RepositoryService.GetDataWithIdAsync([table1Id]);

            return Ok(result);
        }

        [HttpPost("Table1SingleSave")]
        [SwaggerOperation(Description = "儲存單筆 Table1 資料")]
        public async Task<IActionResult> Table1SingleSave([FromBody] Table1Request table1Request)
        {
            var editorName = GetCurrentUserNameFromToken();

            var table1 = _mapper.Map<Table1>(table1Request);
            await _table1RepositoryService.SaveSingleDataAsync(table1, editorName);

            return Ok();
        }

        [HttpPost("Table1MutipleSave")]
        [SwaggerOperation(Description = "儲存多筆 Table1 資料")]
        public async Task<IActionResult> Table1MutipleSave([FromBody] List<Table1Request> table1Requests)
        {
            var editorName = GetCurrentUserNameFromToken();
            var table1s = _mapper.Map<List<Table1>>(table1Requests);
            await _table1RepositoryService.SaveMultipleDataAsync(table1s, editorName);
            return Ok();
        }

        [HttpDelete("DeleteTable1Data")]
        [SwaggerOperation(Description = "刪除指定的 Table1 資料")]
        public async Task<IActionResult> DeleteTable1Data(int table1Id)
        {
            var editorName = GetCurrentUserNameFromToken();
            await _table1RepositoryService.DeleteSingleDataAsync([table1Id], editorName);

            return Ok();
        }

        [HttpGet("GetTable1s")]
        [SwaggerOperation(Description = "取得所有 Table1 資料")]
        public async Task<ActionResult<List<Table1Response>>> GetTable1s()
        {
            var allData = await _table1RepositoryService.GetAllDataAsync();
            var response = _mapper.Map<List<Table1Response>>(allData);
            return Ok(response);
        }

        [HttpGet("FindTable1/{currentPage}/{pageSize}")]
        [SwaggerOperation(Description = "分頁查詢 Table1 資料")]
        public async Task<ActionResult<PagedResult<Table1Response>>> FindTable1(int currentPage, int pageSize, string? querySearch)
        {
            var sortColumns = new List<(string PropertyName, bool IsAscending)> { ("Table1Id", true) };

            var (items, totalCount) = await _table1RepositoryService.FindDataAsync(currentPage, pageSize, querySearch, null, sortColumns);

            var table1List = _mapper.Map<List<Table1Response>>(items);
            var pagedResult = new PagedResult<Table1Response>(table1List, totalCount);
            return Ok(pagedResult);
        }

        /// <summary>
        /// 取得當前JWT Token的使用者名稱，供 RepositoryService 的 Log 紀錄使用
        /// </summary>
        private string GetCurrentUserNameFromToken()
        {
            var nameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)
                ?? throw new InvalidOperationException("JWT Token does not contain Name Claim");

            return nameClaim.Value;
        }
    }
}
