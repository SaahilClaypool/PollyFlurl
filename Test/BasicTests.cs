using System.Net;
using Polly;
using Polly.Flurl;

namespace Test;

public class UnitTest1
{
    [Fact]
    public async Task Test1()
    {
        var policy = Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
            .RetryAsync();
        var x = await new Url("http://www.google.com").WithPolicy(policy).GetStringAsync();

        x.Should().NotBeNullOrEmpty();
    }
}