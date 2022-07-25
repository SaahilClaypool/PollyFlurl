using System.Net;
using System.Text;
using Flurl.Http.Testing;
using Polly;
using Polly.Flurl;
using Xunit.Abstractions;

namespace Test;

public class BasicTests
{
    public BasicTests(ITestOutputHelper output)
    {
        Console.SetOut(new OutputConverter(output));
    }

    [Fact]
    public async Task RetryThenSuccess()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("", status: 500);
        httpTest.RespondWith("", status: 200);

        var response = await "http://www.google.com".WithRetry().GetAsync();
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CustomPolicy_HandleResponse()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("Bad Request", status: 500);
        httpTest.RespondWith("", status: 200);

        var policy = Policy
            .HandleResult<IFlurlResponse>(message =>
            {
                var content = message.GetStringAsync().Result;
                return content == "Bad Request";
            })
            .RetryAsync();

        var response = await "http://www.google.com"
            .AllowAnyHttpStatus() // otherwise raised as an exception
            .WithPolicy(policy)
            .GetAsync();
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CustomPolicy_HandleException()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("Bad Request", status: 500);
        httpTest.RespondWith("", status: 200);

        var policy = Policy
            .Handle<FlurlHttpException>(ex =>
            {
                var content = ex.Call.Response.GetStringAsync().Result;
                return content == "Bad Request";
            })
            .RetryAsync();

        var response = await "http://www.google.com"
            .WithPolicy(policy)
            .GetAsync();
        response.StatusCode.Should().Be(200);
    }
}