using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Watson.Assistant.v2.Model;
using IBM.Watson.Assistant.v2;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SHU_Assistant.Models;
using System;
using System.Diagnostics;
using IBM.Cloud.SDK.Core.Http.Exceptions;
using Environment = System.Environment;
using static System.Net.Mime.MediaTypeNames;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SHU_Assistant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string userQuery)
        {
            try
            {
                string link = "";
                IamAuthenticator authenticator = new IamAuthenticator(
                apikey: "0LTlYh3-Kt6uIe1eQ8ytijsuzdnEKq_jUs8pff49fXeM"
            );

                AssistantService assistant = new AssistantService("2023-01-17", authenticator);
                assistant.SetServiceUrl("https://api.eu-gb.assistant.watson.cloud.ibm.com/instances/9105472d-0990-4acc-a349-661d4607d608");

                var result = assistant.CreateSession(
                    assistantId: "74e78bca-b878-4493-92ad-f31e048b92cd"
                );

                var sessionId = result.Result.SessionId;

                var result2 = assistant.Message(
                    assistantId: "74e78bca-b878-4493-92ad-f31e048b92cd",
                    sessionId,
                    input: new MessageInput()
                    {
                        Text = userQuery
                    }
                );
                Console.WriteLine(result2.Response);
                Dictionary<string, string> dict = new Dictionary<string, string>();

                dynamic jsonResponse = JsonConvert.DeserializeObject(result2.Response);
                ViewBag.MainTitle = jsonResponse.output.generic[0].text;


            
                JObject output = JObject.Parse(result2.Response);
                string answer = (string)output["output"]["generic"][1]["text"];
                ViewBag.MainTitle = (string)output["output"]["generic"][0]["text"];
                if (answer != null)
                {


                    MatchCollection  links = Regex.Matches(answer, @"\[(.*?)\]\(.*?\)");
                    MatchCollection  titles = Regex.Matches(answer, @"(.*?)\s+\(\d");
                    
                    
                    if (links.Count == titles.Count)
                    {
                        for (int i = 0; i < links.Count; i++)
                        {
                            dict.Add(titles[i].Groups[1].Value, links[i].Groups[1].Value);
                        }
                    }
                    else
                    {
                        
                    }

                }
                else
                {
                    ViewBag.MainTitle = "Sorry I could not undrestand what you said please tell me clearly";
                }
                
                ViewBag.TitlesLinks = dict;
                


            }
            catch (ServiceResponseException e)
            {
                Console.WriteLine("Error: " + e.Message);
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}