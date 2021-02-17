using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace RegexTester
{
    public static class MatchRegex
    {
        [FunctionName("MatchRegex")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "match")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var request = await ParseBody(req.Body);

                var result = ProcessRegex(request);

                return new OkObjectResult(result);
            }
            catch (InvalidMatchRequestException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private static MatchResponse ProcessRegex(MatchRequest request)
        {
            try
            {
                var match = Regex.Match(request.Input, request.Pattern, RegexOptions.None, TimeSpan.FromSeconds(2));

                if (match.Success)
                {
                    var response = new MatchResponse()
                    {
                        Success = true
                    };

                    if (!string.IsNullOrWhiteSpace(request.Group) && match.Groups.ContainsKey(request.Group))
                    {
                        response.Value = match.Groups[request.Group].Value;
                    }
                    else
                    {
                        response.Value = match.Value;
                    }

                    return response;
                }
                else
                {
                    return new MatchResponse();
                }
            }
            catch(ArgumentException ex)
            {
                throw new InvalidMatchRequestException("Failed to parse regular expression", ex);
            }
            catch (RegexMatchTimeoutException ex)
            {
                throw new InvalidMatchRequestException("Request has exceeded allowed execution time of 2 seconds.", ex);
            }
        }

        private static async Task<MatchRequest> ParseBody(Stream body)
        {
            if(body == null)
            {
                throw new InvalidMatchRequestException("Request body is null");
            }

            using var bodyReader = new StreamReader(body);
            string requestBody = await bodyReader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                throw new InvalidMatchRequestException("Request body is empty");
            }

            var match = JsonConvert.DeserializeObject<MatchRequest>(requestBody);

            if(match == null)
            {
                throw new InvalidMatchRequestException("Request is not a valid match request");
            }

            if(string.IsNullOrWhiteSpace(match.Input) || string.IsNullOrWhiteSpace(match.Pattern))
            {
                throw new InvalidMatchRequestException("Input and/or Pattern are empty.");
            }

            if(match.Input.Length > 100)
            {
                throw new InvalidMatchRequestException("Input has more than 100 characters");
            }

            if(match.Pattern.Length > 50)
            {
                throw new InvalidMatchRequestException("Pattern has more than 20 characters");
            }

            return match;
        }
    }

    public class MatchRequest
    {
        public string Input { get; set; }
        public string Pattern { get; set; }
        public string Group { get; set; }
    }

    public class MatchResponse
    {
        public bool Success { get; set; }
        public string Value { get; set; }
    }
}
