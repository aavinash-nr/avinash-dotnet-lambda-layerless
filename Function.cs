// using Amazon.Lambda.Core;
// using Amazon.Lambda.APIGatewayEvents;
// using Newtonsoft.Json;
// using NewRelic.Api.Agent;
// using System.Diagnostics;

// // Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
// [assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

// namespace avinash_dotnet_lambda_layerless;

// public class Function
// {
//     private static readonly IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();

//     private static readonly HttpClient httpClient = new HttpClient();
//     public string responseDataString = string.Empty;

//     /// <summary>
//     /// A simple function that takes a string and does a ToUpper
//     /// </summary>
//     /// <param name="input">The event for the Lambda function handler to process.</param>
//     /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
//     /// <returns></returns>
//     public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
//     {
//         var transaction = agent.CurrentTransaction;
//         Console.WriteLine("Starting transaction...");

//         try
//         {
//             Console.WriteLine("Inside try block...");
//             // Simulate an error based on a query parameter
//             if (input.QueryStringParameters != null && input.QueryStringParameters.TryGetValue("error", out string? errorType))
//             {
//                 if (errorType == "httpRequestException")
//                 {
//                     throw new HttpRequestException("Simulated HttpRequestException HTTP request error");
//                 }
//                 else if (errorType == "divideByZeroException")
//                 {
//                     Console.WriteLine("Simulating DivideByZeroException...");
//                     int a= 10;
//                     int b=0;
//                     int c = a/b;
//                 }
//             }

//             var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
//             response.EnsureSuccessStatusCode();

//             var responseBody = await response.Content.ReadAsStringAsync();


//             // Ensure all parameters are properly initialized before setting them
//             var parameters = new Dictionary<string, string>
//             {
//                 { "HttpMethod", input.HttpMethod ?? string.Empty },
//                 { "Path", input.Path ?? string.Empty },
//                 { "Body", input.Body ?? string.Empty }
//             };

//             // Set request parameters in New Relic transaction
//             // transaction.AddCustomAttribute("HttpMethod", input.HttpMethod ?? string.Empty);
//             // transaction.AddCustomAttribute("Path", input.Path ?? string.Empty);
//             // transaction.AddCustomAttribute("Body", input.Body ?? string.Empty);

//             // Your function logic here
//             await Task.Delay(100);


//             return new APIGatewayProxyResponse
//             {
//                 StatusCode = 200,
//                 Body = responseBody,
//                 Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
//             };
//         }
//         catch (DivideByZeroException ex)
//         {
//             NewRelic.Api.Agent.NewRelic.NoticeError(ex);
//             context.Logger.LogLine($"Exception: {ex.Message}");

            
//             // For recrding this error message as custom metrics
//             // NewRelic.Api.Agent.NewRelic.RecordMetric("Custom/"+ex.GetType().ToString(),1);
//             // NewRelic.Api.Agent.NewRelic.IncrementCounter("Custom/"+ex.GetType().ToString());
//             Console.WriteLine("Custom metric recorded for DivideByZeroException");
//             return new APIGatewayProxyResponse
//             {
//                 StatusCode = 500,
//                 Body = JsonConvert.SerializeObject(new { error = ex.Message })
//             };
//         }
//         catch (HttpRequestException exHttpRequestException)
//         {
//             context.Logger.LogLine($"Exception: {exHttpRequestException.Message}");
//             NewRelic.Api.Agent.NewRelic.NoticeError(exHttpRequestException);
//             return new APIGatewayProxyResponse
//             {
//                 StatusCode = 500,
//                 Body = JsonConvert.SerializeObject(new { error = exHttpRequestException.Message })
//             };
//         }
//     }
// }


// public class RequestBody
// {
//     public string? Name { get; set; }
// }

using Amazon.Lambda.Core;
using Newtonsoft.Json;
using NewRelic.Api.Agent;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace avinash_dotnet_lambda_layerless
{
    public class Function
    {
        private static readonly IAgent agent = NewRelic.Api.Agent.NewRelic.GetAgent();

        /// <summary>
        /// Lambda function that simulates a DivideByZeroException.
        /// </summary>
        /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
        /// <returns></returns>
        public string FunctionHandler(ILambdaContext context)
        {
            var transaction = agent.CurrentTransaction;
            try
            {
                Console.WriteLine("Simulating DivideByZeroException...");
                int a = 10;
                int b = 0;
                int c = a / b; 

                return JsonConvert.SerializeObject(new { message = "Success", result = c });
            }
            catch (DivideByZeroException ex)
            {
                var errorAttributes = new Dictionary<string, string>{{"foo", "bar"},{"baz", "luhr"}};

                // Record the error message in string
                string errorMessage = $"Custom DivideByZeroException: {ex.Message}";
                NewRelic.Api.Agent.NewRelic.NoticeError(errorMessage, errorAttributes);
                
                // NewRelic.Api.Agent.NewRelic.NoticeError(ex);

                context.Logger.LogLine($"Exception: {ex.Message}");
                Console.WriteLine("NoticeError recorded for DivideByZeroException");

                return JsonConvert.SerializeObject(new { error = ex.Message });
            }
        }
    }
}