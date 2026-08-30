namespace BlazorAPIProject.Models.Responses.Users
{
    public class AccountResponse
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
