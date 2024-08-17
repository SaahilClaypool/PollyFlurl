# PollyFlurl

[Flurl](https://flurl.dev/) + [Polly](http://www.thepollyproject.org/) = resilient and easy HTTP requests.

- [GitHub](https://github.com/SaahilClaypool/PollyFlurl)
- [Nuget](https://www.nuget.org/packages/PollyFlurl/)

## Examples


```cs
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

```

built in transient retry handler

```cs
var response = await "http://www.google.com".RetryTransientErrors().GetAsync();
```

See [Basic Tests](./Test/BasicTests.cs) for more examples.

## TODO

- Option for global configuration (asp.net core http client factory)