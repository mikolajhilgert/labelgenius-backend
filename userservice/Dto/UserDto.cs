namespace userservice.Dto
{
    public class UserDto
    {
        public string Id;
        public string Email;
    }
    public class UserRegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
