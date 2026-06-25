using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DotNetApiTemplate.DTOs.Requests.User;
using DotNetApiTemplate.DTOs.Responses.Data;
using DotNetApiTemplate.Interfaces;
using DotNetApiTemplate.Models.Entities;
using DotNetApiTemplate.Models.EntityLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetApiTemplate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(
        IRepositoryService<User, UserLog> userRepositoryService,
        UserManager<User> userManager,
        IMapper mapper,
        ILogicService logicService) : ControllerBase
    {
        private readonly IRepositoryService<User, UserLog> _userRepositoryService = userRepositoryService;
        private readonly UserManager<User> _userManager = userManager;
        private readonly IMapper _mapper = mapper;
        private readonly ILogicService _logicService = logicService;

        [HttpGet("FindUsers/{currentPage}/{pageSize}")]
        [SwaggerOperation(Description = "分頁查詢使用者資料")]
        public async Task<ActionResult<PagedResult<UserResponse>>> FindUsers(int currentPage, int pageSize, string? querySearch)
        {
            var sortColumns = new List<(string PropertyName, bool IsAscending)> { ("CreatedAt", false) };
            Expression<Func<User, bool>>? predicate = u => u.IsActive; // 只查詢 isActive 為 true 的使用者
            var (items, totalCount) = await _userRepositoryService.FindDataAsync(currentPage, pageSize, querySearch, predicate, sortColumns);

            var userList = _mapper.Map<List<UserResponse>>(items);
            var pagedResult = new PagedResult<UserResponse>(userList, totalCount);
            return Ok(pagedResult);
        }

        [HttpPut("UpdateUser/{userId}")]
        [SwaggerOperation(Description = "更新使用者基本資料")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var editorName = GetCurrentUserNameFromToken();
                await _logicService.UpdateUserAsync(userId, request, editorName);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("UpdateUserRoles/{userId}")]
        [SwaggerOperation(Description = "更新使用者角色權限 (僅限 Admin)")]
        [Authorize(Roles = "Admin")] // 限制只有 Admin 角色可以調用此 API
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] UpdateUserRolesRequest request)
        {
            try
            {
                var editorName = GetCurrentUserNameFromToken();
                await _logicService.UpdateUserRolesAsync(userId, request.Roles, editorName);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
