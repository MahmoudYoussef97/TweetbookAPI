﻿using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tweetbook.Domain;
using Tweetbook.Options;

namespace Tweetbook.Services
{   
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        public IdentityService(UserManager<IdentityUser> userManager,
            JwtSettings jwtSettings)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
        }
        public async Task<AuthenticationResult> RegisterAsync(string email, string password)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return new AuthenticationResult {
                    Errors = new[] { "User with this email is arleady exists" }
                };
            }
            var newUser = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            return GenerateAuthenticationResultForUser(newUser);
        }
        public async Task<AuthenticationResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new AuthenticationResult { Errors = new[] { "User does not exist" } };

            var userHasValidPassword = await _userManager.CheckPasswordAsync(user, password);

            if (!userHasValidPassword)
                return new AuthenticationResult { Errors = new[] { "User/Passowrd is wrong" } };

            return GenerateAuthenticationResultForUser(user);
        }
        private AuthenticationResult GenerateAuthenticationResultForUser(IdentityUser newUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, newUser.Email),
                    new Claim("Id", newUser.Id)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
    }
}
