 namespace OrderAPI.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string userId, string Email);
    }
}