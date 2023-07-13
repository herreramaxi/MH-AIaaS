namespace AIaaS.WebAPI.Services
{
    public interface IMyCustomService
    {
        Task<bool> IsAuthenticated(string token);
    }
    public class MyCustomService : IMyCustomService
    {
        private readonly IJwtValidationService _jwtValidationService;

        public MyCustomService(IJwtValidationService jwtValidationService)
        {
            _jwtValidationService = jwtValidationService;
        }

        public async Task<bool> IsAuthenticated(string token)
        {
            try
            {
                var principal = await _jwtValidationService.ValidateToken(token);
                var isInRole = principal.IsInRole("AIaaS-consumer");
                //// Access the claims from the validated token
                //var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                //var email = principal.FindFirst(ClaimTypes.Email)?.Value;

                // Perform further processing with the validated token

                return isInRole;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
