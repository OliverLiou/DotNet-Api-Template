using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using JwtAuthDemo.Helpers;
using Microsoft.AspNetCore.Identity;
using DotNetApiTemplate.Models;
using DotNetApiTemplate.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.DirectoryServices.AccountManagement;
using Swashbuckle.AspNetCore.Annotations;

namespace DotNetApiTemplate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IMapper mapper, JwtHelpers jwt, IConfiguration config, UserManager<User> userManager, IPasswordHasher<User> passwordHasher) : ControllerBase
    {
        private IMapper _mapper = mapper;
        private JwtHelpers _jwt = jwt;
        private IConfiguration _config = config;
        private UserManager<User> _userManager = userManager;
        private IPasswordHasher<User> _passwordHasher = passwordHasher;

        
    }
}