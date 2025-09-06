namespace Salubrity.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; init; } = default!;
        public string RefreshToken { get; init; } = default!;
        public DateTime ExpiresAt { get; init; }


        public bool? CampLinked { get; init; }
        public Guid? CampId { get; init; }
        public List<string>? Warnings { get; init; }
        public List<string>? Info { get; init; }

        public AuthResponseDto(string accessToken, string refreshToken, DateTime expiresAt)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }

        public AuthResponseDto() { }
    }

}
