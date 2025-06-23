using Microsoft.IdentityModel.Tokens;

namespace Salubrity.Application.Interfaces.Security
{
    public interface IKeyProvider
    {
        RsaSecurityKey GetPrivateKey();
        RsaSecurityKey GetPublicKey();
    }
}
