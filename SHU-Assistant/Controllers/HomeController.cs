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
        public AssistantService assistant;
        public string sessionId;
        public bool firstTime;
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
            string titleOfText = "";
            string mainTitle = "";
            JArray optionsArray = null;
            List<string> lables = new List<string>();
            bool yesNo = false;
            bool NewOrNot = false;
            string anwser = "";
           

            if (!firstTime)
            {
                try
                {
                    string link = "";
                    IamAuthenticator authenticator = new IamAuthenticator(apikey: "0LTlYh3-Kt6uIe1eQ8ytijsuzdnEKq_jUs8pff49fXeM");

                    AssistantService assis = new AssistantService("2023-01-17", authenticator);
                    assis.SetServiceUrl("https://api.eu-gb.assistant.watson.cloud.ibm.com/instances/9105472d-0990-4acc-a349-661d4607d608");

                    var result = assis.CreateSession(
                        assistantId: "74e78bca-b878-4493-92ad-f31e048b92cd"
                    );

                    var sesionId = result.Result.SessionId;
                    assistant = assis;
                    sessionId = sesionId;


                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                firstTime = true;
            }

            try
            {

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
                JObject response = JObject.Parse(result2.Response);

                if (response != null)
                {

                    //Fetch values from response
                    try
                    {
                        titleOfText = response?["output"]?["generic"]?[0]?["text"]?.ToString();


                        mainTitle = response?["output"]?["generic"]?[0]?["title"]?.ToString();
                    }
                    //If a value is out of range or not found, the override statement is called
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    if (titleOfText != null && titleOfText.Substring(0, 1) == "[")

                    {
                        int index = titleOfText.IndexOf(':');

                        titleOfText = index >= 0 ? titleOfText.Substring(0, index) : titleOfText;
                    }

                    try
                    {
                        titleOfText = response?["output"]?["generic"]?[0]?["text"]?.ToString();


                        mainTitle = response?["output"]?["generic"]?[0]?["title"]?.ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    if (mainTitle != null && mainTitle.Contains("<br />"))
                    {
                        mainTitle = mainTitle.Replace("<br />", "");

                    }
                    if (titleOfText != null && titleOfText.Contains("<br />"))
                    {

                        titleOfText = titleOfText.Replace("<br />", "");
                    }
                    void linkSeprater(JObject response)
                    {
                        for (int i = 0; i < response["output"]["generic"].Count(); i++)
                        {
                            string inputText = response?["output"]?["generic"]?[i]?["text"]?.ToString();
                            string linkPattern = @"\[(.*?)\]\((.*?)\)";
                            Regex regex = new Regex(linkPattern);
                            if (inputText != null)
                            {
                                MatchCollection matches = regex.Matches(inputText);
                                foreach (Match match in matches)
                                {
                                    string link = Regex.Match(match.Value, linkPattern).Groups[2].Value;
                                    string title = match.Groups[1].Value;

                                    dict.Add(title, link);
                                    Console.WriteLine(title + "\n");
                                    Console.WriteLine(link + "\n");
                                }
                            }
                        }
                    }
                    //Console.WriteLine(response);
                    

                    //Fetches values from JSON Response


                    for (int i = 0; i < response?["output"]?["generic"]?[0]?["suggestions"]?.Count(); i++)
                    {

                        string lablles = response?["output"]?["generic"]?[0]?["suggestions"]?[i]?["label"].ToString();
                        lables.Add(lablles);
                        Console.WriteLine(lablles + "\n");
                        
                    }

                    for (int i = 0; i < response?["output"]?["generic"]?.Count(); i++)
                    {
                        optionsArray = response?["output"]?["generic"]?[i]?["options"] as JArray;
                        if (optionsArray != null)
                        {
                            JArray textsArray = response?["output"]?["generic"]?[i]?["text"] as JArray;
                            foreach (JToken option in optionsArray)
                            {
                                string extractedText = (string)option["label"];
                                lables.Add(extractedText);
                                // Output: "I have no knowledge" and "I have some knowledge"
                                Console.WriteLine(extractedText + "\n");
                            }

                        }
                    }

                    foreach(string label in lables)
                    {
                        if (label.ToUpper() == "YES" || label.ToUpper() == "NO")
                        {

                            anwser = "Is there anything else I can help you with?";

                        }
                        if(label.ToLower() == "i'm new to it" || label.ToLower() == "i'm already familiar")
                        {
                            anwser = "How familiar are you with it?";
                        }
                    }
                    ViewBag.MainTitle = mainTitle;
                    ViewBag.MainTextTitle = titleOfText;
                    Console.WriteLine(mainTitle);
                    Console.WriteLine(titleOfText);

                    linkSeprater(response);
                    ViewBag.TitlesLinks = dict;
                    ViewBag.Labels = lables;
                    ViewBag.Question = anwser;
                    
                }
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