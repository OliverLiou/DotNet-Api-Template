using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using  DotNetApiTemplate.Models;
using  DotNetApiTemplate.Services;
using Swashbuckle.AspNetCore.Annotations;

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
            try
            {
                var result = await _repositoryService.GetDataWithIdAsync([table1Id]);

                return Ok(result);
            }
            catch (Exception ex)    
            {
                ModelState.AddModelError("Error", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpPost("Table1Save")]
        [SwaggerOperation(Summary = "儲存單筆 Table1 資料", Description = "儲存單筆 Table1 資料")]
        public async Task<IActionResult> Table1Save(Table1 table1)
        {
            try
            {
                await _repositoryService.SaveSingleDataAsync(table1, "");
                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpPost("Table1MutipleSave")]
        [SwaggerOperation(Summary = "儲存多筆 Table1 資料", Description = "儲存多筆 Table1 資料")]
        public async Task<IActionResult> Table1MutipleSave(List<Table1> table1s)
        {
            try
            {
                await _repositoryService.SaveMutipleDataAsync(table1s, "");
                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpPost("RemoveTable1Data")]
        [SwaggerOperation(Summary = "刪除指定的 Table1 資料", Description = "刪除指定的 Table1 資料")]
        public async Task<IActionResult> RemoveTable1Data(int table1Id)
        {
            try
            {
                await _repositoryService.DeleteSigleDataAsync([table1Id], "");

                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpGet("GetTable1s")]
        [SwaggerOperation(Summary = "取得所有 Table1 資料", Description = "取得所有 Table1 資料")]
        public async Task<IActionResult> GetTable1s()
        {
            try
            {
                var allData = await _repositoryService.GetAllDataAsync();
                return Ok(allData);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return BadRequest(ModelState);
            }
        }

        [HttpGet("FindTable1/{currentPage}/{pageSize}")]
        [SwaggerOperation(Summary = "分頁查詢 Table1 資料", Description = "分頁查詢 Table1 資料")]
        public async Task<IActionResult> FindTable1(int currentPage, int pageSize, string? querySearch)
        {
            try
            {
                // Expression<Func<Table1, bool>> filter = t => t.Column1 == "xxxw666";
                var sortColumns = new List<(string PropertyName, bool IsAscending)> { ("Table1Id", true) };

                var tuple = await _repositoryService.FindDataAsync(currentPage, pageSize, querySearch, null, sortColumns);

                return Ok(tuple);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Error", ex.Message);
                return BadRequest(ModelState);
            }
        }
    }
}