using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WEBAPI_DTOS.DTO;
using WebAPIDemoNew.Data;
using WebAPIDemoNew.Models;
using WebAPIDemoNew.Services;

namespace WebAPIDemoNew.AuthServices
{
    public class AuthService : IAuthService
	{
		private readonly ApplicationDBContext _db;
		private readonly IMapper _mapper;
		private readonly IConfiguration _config;

		public AuthService(ApplicationDBContext db, IMapper mapper, IConfiguration config)
		{
			_db = db;
			_mapper = mapper;
			_config = config;
		}
		public async Task<bool> IsEmailExistsAsync(string email)
		{
			return await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
		}

		public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequestDto)
		{
			try
			{
				var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDto.Email.ToLower());
				if (user == null || user.Password != loginRequestDto.Password)
				{
					return null;
				}
				//generate jwt token
				var token = GenerateJwtToken(user);
				return new LoginResponseDto
				{
					userDto = _mapper.Map<UserDto>(user),
					Token = token
				};
			}
			catch (Exception ex) { 
				throw new InvalidOperationException("An unexcepted error occured during user Login");
			}
		}

		public async Task<UserDto> RegisterAsync(RegistrationRequestDto registrationRequestDto)
		{
			try
			{
				if (await IsEmailExistsAsync(registrationRequestDto.Email))
				{
					throw new InvalidOperationException($"User with email:{registrationRequestDto.Email} already exists");
				}
				User user = new()
				{
					Name = registrationRequestDto.Name,
					Email = registrationRequestDto.Email,
					Password = registrationRequestDto.Password,
					Role = string.IsNullOrEmpty(registrationRequestDto.Role) ? "Customer" : registrationRequestDto.Role,
					CreatedDate = DateTime.Now
				};
				await _db.Users.AddAsync(user);
				await _db.SaveChangesAsync();

				return _mapper.Map<UserDto>(user);
			}
			catch (Exception e) {
				throw new InvalidOperationException("An unexcepted error occured during user registration");
			}
		}

		private string GenerateJwtToken(User user) {
			var key = Encoding.ASCII.GetBytes(_config.GetSection("ApiSettings")["Secret"]);

			var tokenDescriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new[] { 
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.Name, user.Name),
					new Claim(ClaimTypes.Role, user.Role)
				}),
				Expires = DateTime.UtcNow.AddDays(1),
				SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);

		}

	}
}
