using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using API;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.ML;
using Microsoft.ML.Data;

class Program
{
    static void Main()
    {
        var webServer = new WebServer();


        webServer.Requests.Subscribe(LogRequest);
        string stop;

        do
        {

            Console.Write("Enter the YouTube video IDs, separate each with comma (,): ");

            var videoIdsInput = Console.ReadLine();
            var videoIds = videoIdsInput.Split(',').Select(id => id.Trim());


            var youtubeService = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = "AIzaSyDkZCwTdg6DLlbnHPfRrz17J1KV8CFwuKg"
            });

            foreach (var videoId in videoIds)
            {

                var commentThreadsRequest = youtubeService.CommentThreads.List("snippet");
                commentThreadsRequest.VideoId = videoId;

                var commentThreadsResponse = commentThreadsRequest.Execute();
                var realComments = commentThreadsResponse.Items.Select(item => item.Snippet.TopLevelComment.Snippet.TextOriginal);

                Console.WriteLine($"Analyzing comments for video ID: {videoId}");


                foreach (var comment in realComments)
                {
                    MLModel.ModelInput sampleData = new MLModel.ModelInput()
                    {
                        SentimentText = comment
                    };

                    var sentimentResult = MLModel.Predict(sampleData);
                    LogSentimentResult(sentimentResult, comment);
                }
                Console.WriteLine("======================================================");

            }

            do
            {

                Console.WriteLine("Do you want to analyse more videos? yes/no");

                stop = Console.ReadLine();

                if (stop == "yes")
                {
                    stop = "more";
                }
                else if (stop == "no")
                {
                    stop = "stop";
                }
                else
                {
                    stop = "error";
                    Console.WriteLine("Answer needs to be yes or no!!");
                }
            } while (stop == "error");

        } while (stop != "stop");
    }

    static void LogRequest(Request request)
    {
        Console.WriteLine($"New request: VideoId={request.VideoId}, Comment={request.Comment}");
    }

    static void LogSentimentResult(MLModel.ModelOutput result, string comment)
    {
        Console.WriteLine($"Sentiment result: Comment={comment}, Predicted Label={result.PredictedLabel}, Score= {String.Join(",",result.Score)}");
    }
}

class WebServer
{
    private readonly Subject<Request> requestsSubject = new Subject<Request>();

    public IObservable<Request> Requests => requestsSubject.AsObservable();

    public void SimulateIncomingRequest(Request request)
    {
        requestsSubject.OnNext(request);
    }
}

class Request
{
    public string VideoId { get; set; }
    public string Comment { get; set; }
}

