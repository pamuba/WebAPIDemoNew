using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDBContext _db;
		private readonly IMapper _mapper;
		private readonly IConfiguration _config;

		public AuthService(ApplicationDBContext db, IMapper mapper, IConfiguration config, 
			UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_mapper = mapper;
			_config = config;
			_roleManager = roleManager;
			_userManager = userManager;
		}
		public async Task<bool> IsEmailExistsAsync(string email)
		{
			//return await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
			return await _db.ApplicationUsers.AnyAsync(u => u.Email.ToLower() == email.ToLower());
			//_userManager.FindByEmailAsync(email);
		}

		public async Task<TokenDTO?> LoginAsync(LoginRequestDto loginRequestDto)
		{
			try
			{
				var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDto.Email.ToLower());
				if (user == null)
				{
					return null;
				}
				bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

				if (!isValid) {
					return null;
				}
				//generate jwt token
				var token = await GenerateJwtToken(user);
				var roles = await _userManager.GetRolesAsync(user);
				TokenDTO tokenDTO = new TokenDTO
				{
					//userDto = _mapper.Map<UserDto>(user),
					AccessToken = token
				};

				//loginResponseDto.userDto.Role = roles.FirstOrDefault()??"Customer";
				return tokenDTO;
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
				ApplicationUser user = new()
				{
					Name = registrationRequestDto.Name,
					Email = registrationRequestDto.Email,
					UserName = registrationRequestDto.Email,
					//Role = string.IsNullOrEmpty(registrationRequestDto.Role) ? "Customer" : registrationRequestDto.Role,
					//CreatedDate = DateTime.Now
					NormalizedEmail = registrationRequestDto.Email.ToUpper(),
					EmailConfirmed = true
				};

				var result = await _userManager.CreateAsync(user,registrationRequestDto.Password);
				if (!result.Succeeded) {
					var errors = string.Join(", ", result.Errors.Select(e => e.Description));
					throw new InvalidOperationException($"User Registration Failed:{errors}");
				}
				var role = string.IsNullOrEmpty(registrationRequestDto.Role) ? "Customer" : registrationRequestDto.Role;

				if (!await _roleManager.RoleExistsAsync(role)) {
					await _roleManager.CreateAsync(new IdentityRole(role));
				}
				
				await _userManager.AddToRoleAsync(user, role);
				//await _db.Users.AddAsync(user);
				//await _db.SaveChangesAsync();

				var userDto = _mapper.Map<UserDto>(user);
				userDto.Role = role;
				return userDto;
			}
			catch (Exception e) {
				throw new InvalidOperationException("An unexcepted error occured during user registration");
			}
		}

		private async Task<string> GenerateJwtToken(ApplicationUser user) {
			var key = Encoding.ASCII.GetBytes(_config.GetSection("ApiSettings")["Secret"]);
			var roles = await _userManager.GetRolesAsync(user);
			var tokenDescriptor = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(new[] { 
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.Name, user.Name),
					new Claim(ClaimTypes.Role, roles.FirstOrDefault())
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
