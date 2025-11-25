using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace StoreManagementMobile.Services.Auth;

public class TokenHandler : DelegatingHandler
{
    private string? GetToken()
    {
        try
        {
            var vault = new PasswordVault();
            var credential = vault.Retrieve("StoreManagementApp", "AuthToken");
            credential.RetrievePassword();
            return credential.Password;
        }
        catch
        {
            return null;
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
