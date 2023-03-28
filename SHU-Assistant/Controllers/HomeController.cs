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
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.IO;
using System.Web;
using FuzzySharp;
using static System.Net.WebRequestMethods;
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
        public int topicIntendAi = 0;
        public int topicIntendCloud = 0;
        public int topicIntendCyberSec = 0;
        public int topicIntendDesingThink = 0;
        public string[] intents = new string[]{"AI beginners", "AI Intermediate", "Cloud beginners", "Cloud intermediate", "Cyber Security beginners",
        "Cyber Security intermediate", "Design Thinking beginners","Design Thinking intermediate", "AI credentials", "Cloud credentials",
        "Cyber Security credentials", "Design Thinking credentials", "AI Careers", "Cloud Careers", "Cyber Security Careers"};
        public string filePath = Path.GetFullPath("RecomandationDB.csv");

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

                List<string> lines = new List<string>();
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line); // add each line to the list
                    }
                    reader.Close();
                }
                string[] csvArrays = lines.ToArray();


                List<Recomendation> recomendations = new List<Recomendation>();

                foreach (string line in csvArrays)
                {
                    var values = line.Split(",");
                    if (values.Length >= 2 && values != null)
                    {
                        recomendations.Add(new Recomendation(values[0], Convert.ToInt32(values[1])));
                    }
                }

                List<string> recTopics = new List<string>();
                IEnumerable<Recomendation> topThreeRecommendations = recomendations.OrderByDescending(r => r.GetTarget()).Take(3);

                foreach (Recomendation rec in topThreeRecommendations)
                {
                    recTopics.Add(rec.GetLearningTopic());
                }

                if (recTopics != null)
                {
                    ViewBag.Recomendations = recTopics;
                }
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
                Console.WriteLine(result2.Response);

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
                    if (titleOfText != null)

                    {
                        int index = titleOfText.IndexOf(':');

                        titleOfText = index >= 0 ? titleOfText.Substring(0, index) : titleOfText;
                    }
                    if (mainTitle != null)

                    {
                        int index = mainTitle.IndexOf(':');

                        mainTitle = index >= 0 ? mainTitle.Substring(0, index) : mainTitle;
                    }

                    try
                    {

                        for (int i = 0; i < response["output"]["generic"].Count(); i++)
                        {
                            question = response?["output"]?["generic"]?[i]?["text"]?.ToString();
                            if (question != null && question.Contains(","))
                            {

                                if (question != null && question.Substring(0, 1) != "[")

                                {
                                    int index = question.IndexOf(':');

                                    question = index >= 0 ? question.Substring(0, index) : question;
                                }
                                ViewBag.Question = question;
                            }

                            if (question != null && question.Contains("?"))
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
                                    if (!dict.ContainsKey(title))
                                    {
                                        // Check if the value already exists in the dictionary
                                        if (!dict.ContainsValue(link))
                                        {
                                            dict.Add(title, link);
                                            Console.WriteLine(title + "\n");
                                            Console.WriteLine(link + "\n");
                                        }
                                    }
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
                    string topic = "";
                    int topicIntend = 0;
                    // ,AI beginners,0
                    //,AI Intermediate,0
                    //,Cloud beginners,0
                    //,Cloud intermediate,0
                    //,Cyber Security beginners,0
                    //,Cyber Security intermediate,0
                    //,Design Thinking beginners,0
                    //,Design Thinking intermediate,0
                    //,AI credentials,0
                    //,Cloud credentials,0
                    //,Cyber Security credentials,0
                    //,Design Thinking credentials,0
                    //,AI Careers,0
                    //,Cloud Careers,0
                    //,Cyber Security Careers,0

                    if (titleOfText != null && titleOfText.ToUpper().Contains("AI"))
                    {
                        topic = "AI";
                        topicIntendAi++;
                        topicIntend = topicIntendAi;
                    }
                    if (titleOfText != null && titleOfText.ToUpper().Contains("CLOUD"))
                    {
                        topic = "CLOUD";
                        topicIntendCloud++;
                        topicIntend = topicIntendCloud;

                    }
                    if (titleOfText != null && titleOfText.ToUpper().Contains("CYBER SECURITY"))
                    {
                        topic = "CYBER SECURITY";
                        topicIntendCyberSec++;
                        topicIntend = topicIntendCyberSec;

                    }
                    if (titleOfText != null && titleOfText.ToUpper().Contains("DESIGN THINKING"))
                    {
                        topic = "DESIGN THINKING";
                        topicIntendDesingThink++;
                        topicIntend = topicIntendDesingThink;

                    }
                    List<string> lines = new List<string>(); // to store all the lines
                    List<Recomendation> oldRecomendations = new List<Recomendation>();
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines.Add(line); // add each line to the list
                        }
                        reader.Close();
                    }

                    string[] linesArray = lines.ToArray();

                    foreach (string line in linesArray)
                    {
                        var values = line.Split(",");
                        if (values.Length >= 2 && values != null)
                        {
                            oldRecomendations.Add(new Recomendation(values[0], Convert.ToInt32(values[1])));
                        }
                    }

                    Recomendation[] recomendationsArray = oldRecomendations.ToArray();
                    int fuz = 0;
                    foreach (Recomendation recomendation in recomendationsArray)
                    {
                        string learningTopic = recomendation.GetLearningTopic();
                        if (mainTitle != null)
                        {
                            fuz = Fuzz.TokenSortRatio(mainTitle, learningTopic);
                        }
                        if (titleOfText != null)
                        {
                            fuz = Fuzz.TokenSortRatio(titleOfText, learningTopic);
                        }
                        if (fuz > 50)
                        {
                            recomendation.SetTarget(recomendation.GetTarget() + 1);
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.Write(string.Empty);
                        writer.Close();
                    }
                    linkSeprater(response);
                    ViewBag.TitlesLinks = dict;
                    ViewBag.Labels = lables;
                    ViewBag.Anwser = anwser;
                    ViewBag.Topic = topic;
                    foreach (Recomendation re in recomendationsArray)
                    {

                        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath, true))
                        {
                            //write each guest in the new bookings oredered list

                            writer.WriteLine(re.GetLearningTopic() + "," + re.GetTarget());
                            writer.Close();
                        }
                    }

                    //get the file path
                    //D:\SHU\Y2 S1\Professenal Software Project\SHU-Assistant\SHU-Assistant\RecomandationDB.csv

                    List<string> newLines = new List<string>();
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            newLines.Add(line); // add each line to the list
                        }
                        reader.Close();
                    }
                    string[] csvArrays = newLines.ToArray();


                    List<Recomendation> recomendations = new List<Recomendation>();

                    foreach (string line in csvArrays)
                    {
                        var values = line.Split(",");
                        if (values.Length >= 2 && values != null)
                        {
                            recomendations.Add(new Recomendation(values[0], Convert.ToInt32(values[1])));
                        }
                    }

                    List<string> recTopics = new List<string>();
                    IEnumerable<Recomendation> topThreeRecommendations = recomendations.OrderByDescending(r => r.GetTarget()).Take(3);

                    foreach (Recomendation rec in topThreeRecommendations)
                    {
                        recTopics.Add(rec.GetLearningTopic());
                    }

                    if (recTopics != null)
                    {
                        ViewBag.Recomendations = recTopics;
                    }
                }
            }




            catch (ServiceResponseException e)
            {
                Console.WriteLine("Error: " + e.Message);

            }
            //catch (Exception e)
            //{
            //    Console.WriteLine("Error: " + e.Message);
            //}
            return View();
        }
        public ActionResult Profile()
        {
            List<Item> items = new List<Item>();

            items.Add(new Item("https://w7.pngwing.com/pngs/565/373/png-transparent-terraria-team-fortress-2-video-game-weapon-sword-swords-purple-game-angle-thumbnail.png", "Sward1", "Sward1", 500));
            items.Add(new Item("https://w7.pngwing.com/pngs/546/187/png-transparent-terraria-minecraft-true-night-video-game-wikia-minecraft-thumbnail.png", "Sward12", "Sward2", 999));
            items.Add(new Item("https://preview.redd.it/lzalazhjf7i31.jpg?auto=webp&s=5fd106a8bae97b64873ce905e604c13cff363574", "Sward3", "Sward3", 900));
            items.Add(new Item("https://w7.pngwing.com/pngs/860/89/png-transparent-minecraft-pocket-edition-terraria-sword-mod-minecraft-purple-angle-video-game.png", "Sward4", "Sward4", 50));
            items.Add(new Item("https://www.nicepng.com/png/detail/220-2202101_terraria-custom-pixel-art-the-lords-sword-minecraft.png", "Sward5", "Sward5", 10));
            items.Add(new Item("https://www.pngitem.com/pimgs/m/149-1493951_ninja-star-of-terraria-swords-aria-guidelines-2016.png", "Sward6", "Sward6", 200));
            items.Add(new Item("https://www.kindpng.com/picc/m/87-876941_clip-art-diamond-terraria-fire-sword-pixel-art.png", "Sward15", "Sward51", 5));
            items.Add(new Item("https://lh3.googleusercontent.com/ys4nE2iBXT01dLJ3Blgu3dPRKhtXJWZCe3JUoL2PWbatnsdMyduXvlSr6TdECUm6QZQINTGW_t7C-DZNvMkK=s400", "Armor1", "Armor1", 200));
            items.Add(new Item("https://cdna.artstation.com/p/assets/images/images/051/296/252/large/meridianriver-azer-armor-item.jpg?1656948913", "Sward12", "Sward2", 800));
            items.Add(new Item("https://toppng.com/uploads/preview/armor-minecraft-google-pinterest-minecraft-diamond-armor-11563000236eg5w4rh1n5.png", "Armor2", "Armor2", 500));
            items.Add(new Item("https://lh3.googleusercontent.com/2nYsiYgrXrcZdKBZBfzRIqf8OJKwRfp_KS3wg8AUF_wNz4g3vKzJlNksCe7tHs9RJkrnXQvrWIiOpMwDYi6r", "Armor3", "Armor3", 50));
            items.Add(new Item("https://cdn.modrinth.com/data/kvPlmCLX/icon.png", "Armor4", "Armor4", 10));
            items.Add(new Item("https://w7.pngwing.com/pngs/45/40/png-transparent-minecraft-pocket-edition-breastplate-armour-diamond-minecraft-chest-blue-angle-rectangle-thumbnail.png", "Armor6", "Armor6", 999));
            items.Add(new Item("https://www.pngitem.com/pimgs/m/645-6456391_minecraft-armor-png-transparent-png.png", "Armor6", "Sward6", 15));

            ViewBag.Items = items;



            return View();
        }
        public ActionResult Test()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }




    }
    public class Item
    {
        protected string imagePathOfItem;
        protected string imagenameOfItem;
        protected string itemName;
        protected float price;


        public Item(string imagePathOfItem, string imagenameOfItem, string itemName,float price)
        {
            this.imagePathOfItem = imagePathOfItem;
            this.imagenameOfItem = imagenameOfItem;
            this.itemName = itemName;
            this.price = price;
        }

        public string GetImagePath()
        {
            return this.imagePathOfItem;
        }
        public string GetImageName()
        {
            return imagenameOfItem;
        }
        public string GetItemName()
        {
            return itemName;
        }
        public float GetPrice()
        {
            return price;
        }
    }
    public class Recomendation
    {
        protected string learningTopic;
        protected int target;



        public Recomendation(string learningTopic, int target)
        {
            this.learningTopic = learningTopic;
            this.target = target;
        }
        public string GetLearningTopic()
        {
            return learningTopic;
        }

        public int GetTarget()
        {
            return target;
        }

        public void SetTarget(int target)
        {
            this.target = target;
        }
    }
}