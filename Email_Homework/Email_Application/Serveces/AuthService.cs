﻿using Email_Application.AuthServices;
using Email_Domen.Entity.DTOs;
using Email_Domen.Entity.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Email_Application.Serveces
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<string> GenerateToken(UserChecDTO user)
        {
            // Foydalanuvchi mavjudligi tekshiriladi
            if (user == null)
                return "User Not Found";

            // Foydalanuvchi autentifikatsiya amaliyoti muvaffaqiyatli bajarilganligi tekshiriladi
            if (UserExits(user))
            {
                var permissions = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };



                var jsonContent = JsonSerializer.Serialize(permissions);

                // Foydalanuvchi uchun JWT ma'lumotlar to'plami yaratiladi
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Role , "Admin"), // Foydalanuvchi huquqi
                    new Claim("UserName", user.FullName), // Foydalanuvchi nomi
                    new Claim("UserID", user.Id.ToString()), // Foydalanuvchi identifikatori
                    new Claim("CreateDate", DateTime.UtcNow.ToString()),  // Foydalanuvchi yaratilgan vaqti
                    new Claim("Permissions", jsonContent)
                };

                // JWT yaratish uchun ma'lumotlar to'plami bilan yana bir marta funksiya chaqiriladi
                return await GenerateToken(claims);
            }
            else
            {
                var permissions = new List<int>() { 6, 7, 8, 9, 10 };



                var jsonContent = JsonSerializer.Serialize(permissions);

                // Foydalanuvchi uchun JWT ma'lumotlar to'plami yaratiladi
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Role , "User"), // Foydalanuvchi huquqi
                    new Claim("UserName", user.FullName), // Foydalanuvchi nomi
                    new Claim("UserID", user.Id.ToString()), // Foydalanuvchi identifikatori
                    new Claim("CreateDate", DateTime.UtcNow.ToString()),  // Foydalanuvchi yaratilgan vaqti
                    new Claim("Permissions", jsonContent)
                };

                // JWT yaratish uchun ma'lumotlar to'plami bilan yana bir marta funksiya chaqiriladi
                return await GenerateToken(claims);
            }

        }



        public async Task<string> GenerateToken(IEnumerable<Claim> additionalClaims)
        {
            // Xavfsizlik kaliti (key) yaratiladi
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Token muddati (expiry date) olinadi
            var expiryMinutes = Convert.ToInt32(_configuration["JWT:ExpireDate"] ?? "10");

            // Tokenning bazaviy ansambl ma'lumotlari yaratiladi
            var baseClaims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Tokenning ID si
        new Claim(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(DateTime.UtcNow).ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64) // Tokenning yaratilgan vaqti
    };

            // Qo'shimcha ansambl ma'lumotlari tokenning bazaviy ansambliga qo'shiladi
            if (additionalClaims?.Any() == true)
                baseClaims.AddRange(additionalClaims);

            // JWT token yaratiladi
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"], // Token ishlab chiqaruvchisi
                audience: _configuration["JWT:ValidAudience"], // Token qabul qiluvchisi
                claims: baseClaims, // Ansambl ma'lumotlari
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes), // Tokenning muddati
                signingCredentials: credentials // Xavfsizlik kaliti
            );

            // JWT token stringga o'zgartiriladi va qaytariladi
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        private bool UserExits(UserChecDTO model)
        {

            var register = new List<Register>()
            {
                new Register() { Email = "samadovxusan9013@gmail.com", Password = "7777777"},
                new Register() {Email = "clevercoderr@gmail.com", Password = "1111111"}
            };

            foreach (var item in register)
            {
                if (model.Email == item.Email && model.Password == item.Password)
                {
                    return true;
                }
            }
            return false;

        }
    }
}
