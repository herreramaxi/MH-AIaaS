﻿using AIaaS.WebAPI.Data;
using AIaaS.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIaaS.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AIaaSContext _dbContext;

        public UsersController(AIaaSContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<User>> Get()
        {
            return await _dbContext.Users.ToListAsync();
        }
    }
}