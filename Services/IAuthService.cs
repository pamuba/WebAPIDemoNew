using WEBAPI_DTOS.DTO;

namespace WebAPIDemoNew.Services
{
    public interface IAuthService
	{
		Task<UserDto> RegisterAsync(RegistrationRequestDto registrationRequestDto);
		Task<TokenDTO> LoginAsync(LoginRequestDto loginRequestDto);
		Task<bool> IsEmailExistsAsync(string email);
	}
}
