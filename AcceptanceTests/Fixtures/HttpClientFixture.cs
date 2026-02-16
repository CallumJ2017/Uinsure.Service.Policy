using RestSharp;

namespace AcceptanceTests.Fixtures;

public sealed class HttpClientFixture : IDisposable
{
    public RestClient Client { get; }
    public Uri BaseUri { get; }

    public HttpClientFixture()
    {
        BaseUri = new Uri("http://localhost:8080");

        Client = new RestClient(new RestClientOptions
        {
            BaseUrl = BaseUri,
            ThrowOnAnyError = false
        });
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}