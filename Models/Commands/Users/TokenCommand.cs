namespace BlazorAPIProject.Models.Commands.Users
{
    public class TokenCommand
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Email { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
