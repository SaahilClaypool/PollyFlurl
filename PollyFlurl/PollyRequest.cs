using Flurl.Http.Configuration;
using Flurl.Util;

namespace Polly.Flurl;
public class PollyRequest : IFlurlRequest
{
    private readonly IFlurlRequest innerRequest;
    private readonly IAsyncPolicy<IFlurlResponse> policy;

    public PollyRequest(IFlurlRequest innerRequest, IAsyncPolicy<IFlurlResponse> policy)
    {
        this.innerRequest = innerRequest;
        this.policy = policy;
    }
    public IFlurlClient Client { get => innerRequest.Client; set => innerRequest.Client = value; }
    public HttpMethod Verb { get => innerRequest.Verb; set => innerRequest.Verb = value; }
    public Url Url { get => innerRequest.Url; set => innerRequest.Url = value; }

    public IEnumerable<(string Name, string Value)> Cookies => innerRequest.Cookies;

    public CookieJar CookieJar { get => innerRequest.CookieJar; set => innerRequest.CookieJar = value; }
    public FlurlHttpSettings Settings { get => innerRequest.Settings; set => innerRequest.Settings = value; }

    public INameValueList<string> Headers => innerRequest.Headers;

    public Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        return policy.ExecuteAsync(() => innerRequest.SendAsync(verb, content, cancellationToken, completionOption));
    }
}

public class PollyHttpResponseRequest : IFlurlRequest
{
    private readonly IFlurlRequest innerRequest;
    private readonly IAsyncPolicy<HttpResponseMessage> policy;

    public PollyHttpResponseRequest(IFlurlRequest innerRequest, IAsyncPolicy<HttpResponseMessage> policy)
    {
        this.innerRequest = innerRequest;
        this.policy = policy;
    }
    public IFlurlClient Client { get => innerRequest.Client; set => innerRequest.Client = value; }
    public HttpMethod Verb { get => innerRequest.Verb; set => innerRequest.Verb = value; }
    public Url Url { get => innerRequest.Url; set => innerRequest.Url = value; }

    public IEnumerable<(string Name, string Value)> Cookies => innerRequest.Cookies;

    public CookieJar CookieJar { get => innerRequest.CookieJar; set => innerRequest.CookieJar = value; }
    public FlurlHttpSettings Settings { get => innerRequest.Settings; set => innerRequest.Settings = value; }

    public INameValueList<string> Headers => innerRequest.Headers;

    public async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead)
    {
        var response = await policy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await innerRequest.SendAsync(verb, content, cancellationToken, completionOption);
                return response.ResponseMessage;
            }
            catch (FlurlHttpException ex)
            {
                return ex.Call.Response.ResponseMessage;
            }
        });

        return new FlurlResponse(response);
    }
}