﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1
{
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }
        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody] UserRegisterationRequest request)
        {
            var authResponse = await _identityService.RegisterAsync(request.Email, request.Password);

            if (!authResponse.Success)
                return BadRequest(new AuthFailedResponse { Errors = authResponse.Errors });

            return Ok(new AuthSuccessResponse { Token = authResponse.Token });
        }
        [HttpPost(ApiRoutes.Identity.LogIn)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {   
            if(!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse 
                { 
                    Errors = ModelState.Values.SelectMany(e => e.Errors.Select(er => er.ErrorMessage)) 
                });
            }

            var authResponse = await _identityService.LoginAsync(request.Email, request.Password);

            if (!authResponse.Success)
                return BadRequest(new AuthFailedResponse { Errors = authResponse.Errors });

            return Ok(new AuthSuccessResponse { Token = authResponse.Token });
        }
    }
}
