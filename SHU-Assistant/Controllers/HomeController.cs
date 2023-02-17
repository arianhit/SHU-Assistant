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
using Microsoft.AspNetCore.Http;
using IBM.Cloud.SDK.Core.Authentication;
//challanges 
//Every time user chat with bot new session will creat
//The options that bot give needed to be button on the page so user should be able to click on it and send the label of the button to bot 
//when user click on the option buttons the textbox will automaticlly fill with the label of the button which user clicked and then submit it by it selfS
//open graph tags 
//og stuff 
namespace SHU_Assistant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public AssistantService assistant;
        public string sessionId;

        public HomeController(ILogger<HomeController> logger)
        {
            IamAuthenticator authenticator = new IamAuthenticator(apikey: "0LTlYh3-Kt6uIe1eQ8ytijsuzdnEKq_jUs8pff49fXeM");
            AssistantService assis = new AssistantService("2023-01-17", authenticator);
            assis.SetServiceUrl("https://api.eu-gb.assistant.watson.cloud.ibm.com/instances/9105472d-0990-4acc-a349-661d4607d608");
            assistant = assis;
            _logger = logger;
        }

        public IActionResult Index()
        {

            string link = "";

            Console.WriteLine(sessionId);

            // Pass the session ID to the view
            try
            {





                // Remove the session ID from the cookie
                HttpContext.Response.Cookies.Delete("sessionId");

                // Create a new session and store the session ID in the session cookie
                var result1 = assistant.CreateSession(assistantId: "74e78bca-b878-4493-92ad-f31e048b92cd");
                sessionId = result1.Result.SessionId;
                Console.WriteLine(sessionId);
                HttpContext.Response.Cookies.Append("sessionId", sessionId);




            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
            string question = "";







            try
            {
                string sessionId = HttpContext.Request.Cookies["sessionId"];

                var result2 = assistant.Message(
        assistantId: "74e78bca-b878-4493-92ad-f31e048b92cd",
        sessionId: sessionId,
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
                    if (titleOfText != null && titleOfText.Contains(":"))

                    {
                        int index = titleOfText.IndexOf(':');

                        titleOfText = index >= 0 ? titleOfText.Substring(0, index) : titleOfText;
                    }

                    try
                    {
                        
                        for (int i = 0; i < response["output"]["generic"].Count(); i++)
                        {
                            question = response?["output"]?["generic"]?[i]?["text"]?.ToString();
                            if (question != null && question.Contains("?"))
                            {
                                ViewBag.Question = question;
                            }
                            if (question != null && question.Contains(","))
                            {
                                ViewBag.Question = question;
                            }
                            if(question != null && !question.Contains("http"))
                            {
                                ViewBag.Question = question;
                            }
                        }


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

                  
                    ViewBag.MainTitle = mainTitle;
                    ViewBag.MainTextTitle = titleOfText;
                    Console.WriteLine(mainTitle);
                    Console.WriteLine(titleOfText);

                    linkSeprater(response);
                    ViewBag.TitlesLinks = dict;
                    ViewBag.Labels = lables;
                    ViewBag.Anwser = anwser;
                    
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