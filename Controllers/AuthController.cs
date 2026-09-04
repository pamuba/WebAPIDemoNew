using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using WEBAPI_DTOS.DTO;
using WebAPIDemoNew.Services;

namespace WebAPIDemoNew.Controllers
{
    [Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService authService;
		public AuthController(IAuthService authService)
		{
			this.authService = authService;
		}

		[HttpPost]
		[Route("register")]
		[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<ActionResult<ApiResponse<UserDto>>> Register([FromBody]RegistrationRequestDto registrationRequestDto)
		{
			try
			{
				if (registrationRequestDto == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Registration Data is Required."));
				}
				if (await authService.IsEmailExistsAsync(registrationRequestDto.Email))
				{
					return Conflict(ApiResponse<object>.Conflict($"User with email:'{registrationRequestDto.Email}' already exists"));
				}

				var user = await authService.RegisterAsync(registrationRequestDto);

				if (user == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Registration Failed"));
				}
				//auth service
				var response = ApiResponse<UserDto>.CreatedAt(user, "User Created Successfully");
				return CreatedAtAction(nameof(Register), response);
			}
			catch (Exception ex) {
			{
					var errorMessage = ApiResponse<object>.Error(500, "An error occured during registration", ex.Message);
					return StatusCode(500, errorMessage);
			}
		}
		}
		[HttpPost]
		[Route("login")]
		[ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
		public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto  loginRequestDto)
		{
			try
			{
				if (loginRequestDto == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Login Data is Required."));
				}

				var loginResponse = await authService.LoginAsync(loginRequestDto);

				if (loginResponse == null)
				{
					return BadRequest(ApiResponse<object>.BadRequest("Login Failed"));
				}
				//auth service
				var response = ApiResponse<LoginResponseDto>.OK(loginResponse, "Loggedin Successfully");
				return Ok(response);
			}
			catch (Exception ex)
			{
				{
					var errorMessage = ApiResponse<object>.Error(500, "An error occured during Logging in", ex.Message);
					return StatusCode(500, errorMessage);
				}
			}
		}


	}
}
