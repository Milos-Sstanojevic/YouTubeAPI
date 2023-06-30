using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Net.Sockets;
using System.Net;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System;
using API1;

class Program
{
    static async Task Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8083);
        listener.Start();
        Console.WriteLine("Waiting for requests on port 8083...");

        var connectionObservable = Observable.FromAsync(() => listener.AcceptTcpClientAsync())
            .Repeat()
            .Publish();

        using (var connectionDisposable = connectionObservable.Connect())
        {
            connectionObservable.Subscribe(async client =>
            {
                try
                {
                    await ProcessRequest(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing request: " + ex.Message);
                }
            });

            await Task.Delay(-1);
        }
    }

    static async Task ProcessRequest(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream);

        try
        {
            string request = await reader.ReadLineAsync();

            if (request != null)
            {
                Console.WriteLine("Received request: " + request);

                string[] parts = Regex.Split(request, @"\s+");

                if (parts.Length == 3 && parts[0] == "GET")
                {
                    string videoId = parts[1].Substring(1);

                    #region "GettingComments"

                    Console.WriteLine("Video ID: " + videoId);

                    var youtubeService = new YouTubeService(new BaseClientService.Initializer
                    {
                        ApiKey = "OVDE IDE API YOUTUBE KEY"
                    });

                    var commentThreadsRequest = youtubeService.CommentThreads.List("snippet");
                    commentThreadsRequest.VideoId = videoId;
                    commentThreadsRequest.MaxResults = 150;

                    var commentThreadsResponse = await commentThreadsRequest.ExecuteAsync();

                    var realComments = commentThreadsResponse.Items
                        .Select(item => item.Snippet.TopLevelComment.Snippet.TextOriginal);

                    Console.WriteLine($"Analyzing comments for video ID: {videoId}");

                    StringBuilder responseBuilder = new StringBuilder();

                    responseBuilder.AppendLine("<table>");
                    responseBuilder.AppendLine("<tr><th>Comment</th><th>Result</th><th>Chance Positive</th><th>Chance Negative</th></tr>");

                    foreach (var comment in realComments)
                    {
                        MLModel.ModelInput sampleData = new MLModel.ModelInput()
                        {
                            Col0 = comment
                        };

                        var predictionResult = MLModel.Predict(sampleData);


                        string toxicLabel = predictionResult.PredictedLabel == 1 ? "Positive" : "Negative";
                        double confidenceT = predictionResult.Score[0] * 100;
                        double confidenceF = predictionResult.Score[1] * 100;
                        responseBuilder.AppendLine($"<tr><td>{comment}</td><td>{toxicLabel}</td><td>{confidenceT:0}%</td><td>{confidenceF:0}%</td></tr>");
                    }

                    #endregion

                    responseBuilder.AppendLine("</table>");

                    string response = responseBuilder.ToString();

                    WriteResponse(response, writer);
                }
                else
                {
                    Console.WriteLine("Bad request: " + request);
                    WriteBadRequestResponse(writer);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error processing request: " + ex.Message);
            WriteServerErrorResponse(writer);
        }
        finally
        {
            writer.Flush();
            writer.Close();
            reader.Close();
            stream.Close();
            client.Close();
        }
    }

    static void WriteResponse(string response, StreamWriter writer)
    {
        writer.WriteLine("HTTP/1.1 200 OK");
        writer.WriteLine("Content-Type: text/html; charset=UTF-8");
        writer.WriteLine("Connection: close");
        writer.WriteLine();
        writer.WriteLine("<!DOCTYPE html>");
        writer.WriteLine("<html>");
        writer.WriteLine("<head>");
        writer.WriteLine("<title>Sentiment Analysis Results</title>");
        writer.WriteLine("<style>");
        writer.WriteLine("table { border-collapse: collapse; width: 100%; }");
        writer.WriteLine("th, td { text-align: left; padding: 8px; }");
        writer.WriteLine("tr:nth-child(even) { background-color: #f2f2f2; }");
        writer.WriteLine("</style>");
        writer.WriteLine("</head>");
        writer.WriteLine("<body>");
        writer.WriteLine("<h1>Sentiment Analysis Results</h1>");
        writer.WriteLine(response);
        writer.WriteLine("</body>");
        writer.WriteLine("</html>");
    }

    static void WriteBadRequestResponse(StreamWriter writer)
    {
        writer.WriteLine("HTTP/1.1 400 Bad Request");
        writer.WriteLine("Content-Type: text/plain; charset=UTF-8");
        writer.WriteLine("Connection: close");
        writer.WriteLine();
        writer.WriteLine("Bad Request");
    }

    static void WriteServerErrorResponse(StreamWriter writer)
    {
        writer.WriteLine("HTTP/1.1 500 Internal Server Error");
        writer.WriteLine("Content-Type: text/plain; charset=UTF-8");
        writer.WriteLine("Connection: close");
        writer.WriteLine();
        writer.WriteLine("Internal Server Error");
    }
}
