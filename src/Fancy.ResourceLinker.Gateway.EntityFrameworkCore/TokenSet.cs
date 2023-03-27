using System.ComponentModel.DataAnnotations;

namespace Fancy.ResourceLinker.Gateway.EntityFrameworkCore;

internal class TokenSet
{
    [Key]
    public string UserId { get; set; }
    public string IdToken { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
