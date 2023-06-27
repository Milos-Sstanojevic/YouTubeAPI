using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using API;
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
                    commentThreadsRequest.MaxResults = 150;

                    var commentThreadsResponse = commentThreadsRequest.Execute();
                    var realComments = commentThreadsResponse.Items.Select(item => item.Snippet.TopLevelComment.Snippet.TextOriginal);

                    Console.WriteLine($"Analyzing comments for video ID: {videoId}");

                    StringBuilder responseBuilder = new StringBuilder();

                    responseBuilder.AppendLine("<table>");
                    responseBuilder.AppendLine("<tr><th>Comment</th><th>Result</th><th>Chance it's Toxic</th></tr>");


                    foreach (var comment in realComments)
                    {
                        MLModel.ModelInput sampleData = new MLModel.ModelInput()
                        {
                            Text = comment
                        };

                        var predictionResult = MLModel.Predict(sampleData);

                        string toxicLabel = (predictionResult.PredictedLabel) == "TRUE" ? "Toxic" : "Non-Toxic";
                        Console.WriteLine(toxicLabel);
                        double confidence = predictionResult.Score[0] * 100;

                        responseBuilder.AppendLine($"<tr><td>{comment}</td><td>{toxicLabel}</td><td>{confidence:0}%</td></tr>");

                    }

                    responseBuilder.AppendLine("</table>");

                    string response = responseBuilder.ToString();

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
            Console.WriteLine("Greska sa obsluzivanjem zahteva: " + ex.Message);
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