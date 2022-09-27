using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ScrapingWeb.Models;
using HtmlAgilityPack;
using System.Net;
using System.Text;
namespace ScrapingWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    public IActionResult Index()
    {
        // string url = "https://en.wikipedia.org/wiki/List_of_programmers";
        string url = "https://market.nbebank.com/market/banks/index.php";
        string response = CallUrl(url).Result;

        // var linkList = ParseHtml(response);

        // HtmlDocument htmlDoc = new HtmlDocument();
        // var linkListXPATH =   htmlDoc.DocumentNode.SelectNodes("//li[not(contains(@class, 'tocsection'))]");

        // WriteToCsv(linkList);

       var curenciesFromNBE =  ParseHtmlCurrency(response);

       PrintToConsole(curenciesFromNBE);

        return View();
    }

    private static async Task<string> CallUrl(string fullUrl)
    {
        HttpClient client = new HttpClient();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;
        client.DefaultRequestHeaders.Accept.Clear();
        var response = client.GetStringAsync(fullUrl);
        return await response;
    }
    // private List<string> ParseHtmlXPATH(){
        
    //     HtmlDocument htmlDoc = new HtmlDocument();
    //   return  htmlDoc.DocumentNode.SelectNodes("//li[not(contains(@class, 'tocsection'))]");

    // }  

    private List<string> ParseHtml(string html)
    {
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var programmerLinks = htmlDoc.DocumentNode.Descendants("li")
            .Where(node => !node.GetAttributeValue("class", "").Contains("tocsection"))
            .ToList();

        List<string> wikiLink = new List<string>();

        foreach (var link in programmerLinks)
        {
            if (link.FirstChild.Attributes.Count > 0)
                wikiLink.Add("https://en.wikipedia.org/" + link.FirstChild.Attributes[0].Value);
        }

        return wikiLink;
    }
    private List<CurrencyDataNBE> ParseHtmlCurrency(string html)
    {
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var programmerLinks = htmlDoc.DocumentNode.Descendants("table").ToList();

        List<string> wikiLink = new List<string>();
        List<CurrencyDataNBE> currencyDataNBEs = new List<CurrencyDataNBE>();
        // int i = 0;
        //loop trough each table(each currency) each currency is in a table element with tbody and tr and td elements inside it 
        for (int i = 0; i < programmerLinks.Count; i++)
        {
             var currency = new CurrencyDataNBE();
            if (i < 4)
            {
                continue;
            }
                // Inside a specific Currency 
                // get table row 
             var w = programmerLinks[i].Descendants("tr");
             foreach (var item in w)
             {
                // get all td elements inside the tr element
                var td = item.Descendants("td").ToList();
                for (int j = 0; j < td.Count(); j++)
                {
                    // skip the first td its just empty space
                    if (j!=0)
                    {
                       var ttt =  td[j].InnerHtml;
                        var indexSemicolon = ttt.IndexOf(";") + 1;
                        var ioio = ttt.Substring(indexSemicolon).Trim();
                        if (j ==1)
                        {
                            currency.Name = ioio;
                        }
                         if (j ==2)
                        {
                            currency.Buying = ioio;
                        }
                         if (j ==3)
                        {
                            currency.Selling = ioio;
                        }
                        
                    //    var cle = ttt.Replace(System.Environment.NewLine,"");
                    }
                }
             currencyDataNBEs.Add(currency);
             }
        }

        return currencyDataNBEs;
    }

    private void WriteToCsv(List<string> links)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var link in links)
        {
            sb.AppendLine(link);
        }

        System.IO.File.WriteAllText("links.csv", sb.ToString());
    }

     private void PrintToConsole(List<CurrencyDataNBE> currencyDataNBEs)
    {
        // StringBuilder sb = new StringBuilder();
        //  System.Console.WriteLine($"  Name            Buying            Selling");
                    System.Console.WriteLine("{0,-25}{1,-13}{2,-13}","Currency Name", "Buying", "Selling");


        foreach (var item in currencyDataNBEs)
        {
        //    System.Console.WriteLine($"  {item.Name}            {item.Buying}            {item.Selling}");
           System.Console.WriteLine("{0,-25}{1,-13}{2,-13}",item.Name, item.Buying, item.Selling);
        }

        // System.IO.File.WriteAllText("links.csv", sb.ToString());
    }
    


}


public class CurrencyDataNBE
{
    public string Name { get; set; }
    public string Buying { get; set; }

    public string Selling { get; set; }
}