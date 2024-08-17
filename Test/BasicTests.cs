using System.Net;
using System.Text;
using Flurl.Http.Testing;
using Polly;
using Polly.Retry;
using PollyFlurl;
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

        var response = await "http://www.google.com".RetryTransientErrors().GetAsync();
        response.StatusCode.Should().Be(200);
    }

    #region ResiliencePipeline

    [Fact]
    public async Task CustomPipeline_HandleHTTPResponseMessage()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("Bad Request", status: 500);
        httpTest.RespondWith("", status: 200);

        var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>()
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(message =>
                    {
                        var content = message.Content.ReadAsStringAsync().Result;
                        return content == "Bad Request";
                    })
            })
            .Build();

        var response = await "http://www.google.com"
            .WithPipeline(pipeline)
            .GetAsync();
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CustomPipeline_HandleResponse()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("Bad Request", status: 500);
        httpTest.RespondWith("", status: 200);

        var pipeline = new ResiliencePipelineBuilder<IFlurlResponse>()
            .AddRetry(new RetryStrategyOptions<IFlurlResponse>()
            {
                ShouldHandle = new PredicateBuilder<IFlurlResponse>()
                    .HandleResult(message =>
                    {
                        var content = message.GetStringAsync().Result;
                        return content == "Bad Request";
                    })
            })
            .Build();

        var response = await "http://www.google.com"
            .AllowAnyHttpStatus() // otherwise raised as an exception
            .WithPipeline(pipeline)
            .GetAsync();
        response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CustomPipeline_HandleException()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("Bad Request", status: 500);
        httpTest.RespondWith("", status: 200);

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions()
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<FlurlHttpException>(ex =>
                    {
                        var content = ex.Call.Response.GetStringAsync().Result;
                        return content == "Bad Request";
                    })
            })
            .Build();

        var response = await "http://www.google.com"
            .WithPipeline(pipeline)
            .GetAsync();
        response.StatusCode.Should().Be(200);
    }

    #endregion

    #region Policy (legacy)

    [Fact]
    public async Task CustomPolicy_HandleHTTPResponseMessage()
    {
        using var httpTest = new HttpTest();
        httpTest.RespondWith("Bad Request", status: 500);
        httpTest.RespondWith("", status: 200);

        var policy = Policy
            .HandleResult<HttpResponseMessage>(message =>
            {
                var content = message.Content.ReadAsStringAsync().Result;
                return content == "Bad Request";
            })
            .RetryAsync();

#pragma warning disable CS0618 // ".WithPolicy()" is obsolete
        var response = await "http://www.google.com"
            .WithPolicy(policy)
            .GetAsync();
#pragma warning restore CS0618 // ".WithPolicy()" is obsolete
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

#pragma warning disable CS0618 // ".WithPolicy()" is obsolete
        var response = await "http://www.google.com"
            .AllowAnyHttpStatus() // otherwise raised as an exception
            .WithPolicy(policy)
            .GetAsync();
#pragma warning restore CS0618 // ".WithPolicy()" is obsolete
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

#pragma warning disable CS0618 // ".WithPolicy()" is obsolete
        var response = await "http://www.google.com"
            .WithPolicy(policy)
            .GetAsync();
#pragma warning restore CS0618 // ".WithPolicy()" is obsolete
        response.StatusCode.Should().Be(200);
    }

    #endregion
}