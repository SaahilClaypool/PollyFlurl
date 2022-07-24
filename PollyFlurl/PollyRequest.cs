using Flurl.Http.Configuration;
using Flurl.Util;

namespace Polly.Flurl;
public class PollyRequest : IFlurlRequest
{
    private readonly IFlurlRequest originalRequest;
    private readonly IAsyncPolicy<IFlurlResponse> policy;

    public PollyRequest(IFlurlRequest originalRequest, IAsyncPolicy<IFlurlResponse> policy)
    {
        this.originalRequest = originalRequest;
        this.policy = policy;
    }
    public IFlurlClient Client { get => originalRequest.Client; set => originalRequest.Client = value; }
    public HttpMethod Verb { get => originalRequest.Verb; set => originalRequest.Verb = value; }
    public Url Url { get => originalRequest.Url; set => originalRequest.Url = value; }

    public IEnumerable<(string Name, string Value)> Cookies => originalRequest.Cookies;

    public CookieJar CookieJar { get => originalRequest.CookieJar; set => originalRequest.CookieJar = value; }
    public FlurlHttpSettings Settings { get => originalRequest.Settings; set => originalRequest.Settings = value; }

    public INameValueList<string> Headers => originalRequest.Headers;

    public Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return policy.ExecuteAsync(() => originalRequest.SendAsync(verb, content, cancellationToken, completionOption));
    }
}