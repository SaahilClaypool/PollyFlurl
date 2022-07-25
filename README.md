# PollyFlurl

[Flurl](https://flurl.dev/) + [Polly](http://www.thepollyproject.org/) = resilient and easy HTTP requests.

[related github issue](https://github.com/tmenier/Flurl/issues/346)

## Examples


```cs
var policy = Policy
    .HandleResult<HttpResponseMessage>(message =>
    {
        var content = message.Content.ReadAsStringAsync().Result;
        return content == "Bad Request";
    })
    .RetryAsync();

var response = await "http://www.google.com"
    .WithPolicy(policy)
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
    - Create a custom factory that supports injecting a flurl 