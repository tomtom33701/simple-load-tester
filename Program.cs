using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


string GetInputFor(string msg, string defaultVal)
{
    Console.Write(msg);
    if (Console.ReadLine() is var input && !string.IsNullOrWhiteSpace(input))
        return input;
    return defaultVal;
}

const string defaultEndpoint = "agent/conversations/89556bb3-962c-4c74-8284-4cd36bbbbbbb/dispositions";

var portNum =GetInputFor("Enter port number (default is 5000): ", "5000");
Console.WriteLine($"Port number set to {portNum}");

var requestUri = GetInputFor("Enter request URI or press ENTER for default endpoint: ", defaultEndpoint);
Console.WriteLine($"Request URI set to {requestUri}");

var baseUri = new Uri($"http://localhost:{portNum}/api/");

var authHeader = GetInputFor("Enter auth header token (ENTER for default): ", "D5j3T7wpkKpoVCcZVemQFfbt4aQXqzBIj_3ZJQNg");

var numOfRequest = GetInputFor("Enter number of requests (Default is 10,000): ", "10000");
var requestCount = int.Parse(numOfRequest);

var threads = GetInputFor("Enter number of threads (Default is 10): ", "10");
var numOfThreads = int.Parse(threads);


var client = new HttpClient();

client.DefaultRequestHeaders.Add("Authorization-AgentCommunication", authHeader);
client.BaseAddress = baseUri;

var cancelToken = new CancellationTokenSource();
Console.WriteLine("Getting ready to execute.  Press Control + C to cancel...");
Thread.Sleep(5000);

// Use Ctl + C to stop the application
Console.CancelKeyPress += (_, _) =>
{
    cancelToken.Cancel();
    Console.WriteLine("Cancelling...");
};

Parallel.For(0, requestCount, new ParallelOptions {MaxDegreeOfParallelism = numOfThreads, CancellationToken =cancelToken.Token }, i =>
{
    Console.WriteLine($"Preparing invocation count: {i}. Uri: {requestUri}");
    try
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Add("Authorization-AgentCommunication", Guid.NewGuid().ToString());
        var response = client.SendAsync(request);
        response.Wait(5000);
        using var r = response.Result.Content.ReadAsStringAsync();
        r.Wait();
        Console.WriteLine($"Result Code: {response.Result.StatusCode}");
        response.Result.Dispose();

    }
    catch (Exception ex)
    {
        Console.WriteLine($"Caught exception: {ex.Message}");
    }
    finally
    {
        Console.WriteLine($"Invocation count {i} complete...");
    }

});


