using Flurl.Http.Configuration;
using Flurl.Util;

namespace PollyFlurl;

internal abstract class RequestWrapper : IFlurlRequest
{
    protected readonly IFlurlRequest innerRequest;

    public RequestWrapper(IFlurlRequest innerRequest)
    {
        this.innerRequest = innerRequest;
    }
    public IFlurlClient Client { get => innerRequest.Client; set => innerRequest.Client = value; }
    public HttpMethod Verb { get => innerRequest.Verb; set => innerRequest.Verb = value; }
    public Url Url { get => innerRequest.Url; set => innerRequest.Url = value; }

    public IEnumerable<(string Name, string Value)> Cookies => innerRequest.Cookies;

    public CookieJar CookieJar { get => innerRequest.CookieJar; set => innerRequest.CookieJar = value; }
    public FlurlHttpSettings Settings { get => innerRequest.Settings; set => innerRequest.Settings = value; }

    public INameValueList<string> Headers => innerRequest.Headers;

    public abstract Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default);
}

/// <summary> Wrap a flurl request policy</summary>
internal class PollyRequestFlurlResponse : RequestWrapper
{
    private readonly IAsyncPolicy<IFlurlResponse> policy;

    public PollyRequestFlurlResponse(IFlurlRequest innerRequest, IAsyncPolicy<IFlurlResponse> policy) : base(innerRequest)
    {
        this.policy = policy;
    }

    public override Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default) =>
        policy.ExecuteAsync(() => innerRequest.SendAsync(verb, content, completionOption, cancellationToken));
}

/// <summary> Wrap a generic policy </summary>
internal class PollyRequest : RequestWrapper
{
    private readonly IAsyncPolicy policy;

    public PollyRequest(IFlurlRequest innerRequest, IAsyncPolicy policy) : base(innerRequest)
    {
        this.policy = policy;
    }

    public override Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default) =>
        policy.ExecuteAsync(() => innerRequest.SendAsync(verb, content, completionOption, cancellationToken));
}

/// <summary> Wrap a http response policy </summary>
internal class PollyHttpResponseRequest : RequestWrapper
{
    private readonly IAsyncPolicy<HttpResponseMessage> policy;

    public PollyHttpResponseRequest(IFlurlRequest innerRequest, IAsyncPolicy<HttpResponseMessage> policy) : base(innerRequest)
    {
        this.policy = policy;
    }

    public override async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent? content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default)
    {
        var response = await policy.ExecuteAsync(async () =>
        {
            try
            {
                var response = await innerRequest.SendAsync(verb, content, completionOption, cancellationToken);
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