using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using API1;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

class Program
{
    static void Main()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, 8083);
        listener.Start();
        Console.WriteLine("Cekam zahtev sa porta 8083...");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient();
            ProcessRequest(client);
        }
    }

    static void ProcessRequest(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream);

        try
        {
            string request = reader.ReadLine();

            if (request != null)
            {
                Console.WriteLine("Primljeni zahtev: " + request);

                string[] parts = Regex.Split(request, @"\s+");

                if (parts.Length == 3 && parts[0] == "GET")
                {
                    string videoId = parts[1].Substring(1);

                    Console.WriteLine("Id videa je: " + videoId);

                    var youtubeService = new YouTubeService(new BaseClientService.Initializer
                    {
                        ApiKey = "AIzaSyDkZCwTdg6DLlbnHPfRrz17J1KV8CFwuKg"
                    });

                    var commentThreadsRequest = youtubeService.CommentThreads.List("snippet");
                    commentThreadsRequest.VideoId = videoId;
                    commentThreadsRequest.MaxResults = 50;

                    var commentThreadsResponse = commentThreadsRequest.Execute();
                    var realComments = commentThreadsResponse.Items.Select(item => item.Snippet.TopLevelComment.Snippet.TextOriginal);

                    Console.WriteLine($"Analyzing comments for video ID: {videoId}");

                    StringBuilder responseBuilder = new StringBuilder();

                    string[] labels = { "IsToxic", "IsAbusive", "IsProvocal" };

                    foreach (var comment in realComments)
                    {
                        MLModel.ModelInput sampleData = new MLModel.ModelInput()
                        {
                            Text = comment
                        };

                        var sentimentResult = MLModel.Predict(sampleData);

                        string formattedLabel = $"<span style=\"color:red;\"><b>Is Toxic={sentimentResult.PredictedLabel}, Chance that it's not toxic={Convert.ToDouble(sentimentResult.Score[0])*100:0}%</b></span>";

                         responseBuilder.AppendLine($"Sentiment result: Comment={comment}, {formattedLabel}");


                    }
                    string response = responseBuilder.ToString();

                    // Build the HTTP response
                    writer.WriteLine("HTTP/1.1 200 OK");
                    writer.WriteLine("Content-Type: text/html; charset=UTF-8");
                    writer.WriteLine("Connection: close");
                    writer.WriteLine();
                    writer.WriteLine("<!DOCTYPE html>");
                    writer.WriteLine("<html>");
                    writer.WriteLine("<head>");
                    writer.WriteLine("<title>Sentiment Analysis Results</title>");
                    writer.WriteLine("</head>");
                    writer.WriteLine("<body>");
                    writer.WriteLine("<h1>Sentiment Analysis Results</h1>");
                    writer.WriteLine("<pre>");
                    writer.WriteLine(response);
                    writer.WriteLine("</pre>");
                    writer.WriteLine("</body>");
                    writer.WriteLine("</html>");

                    writer.Flush();
                }
                else
                {
                    Console.WriteLine("Los zahtev: " + request);

                    writer.Write("HTTP/1.1 400 Bad Request\r\n");
                    writer.Write("\r\n");
                    writer.Flush();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Gresak sa obsluzivanjem zahteva: " + ex.Message);
        }
        finally
        {
            writer.Close();
            reader.Close();
            stream.Close();
            client.Close();
        }
    }
}