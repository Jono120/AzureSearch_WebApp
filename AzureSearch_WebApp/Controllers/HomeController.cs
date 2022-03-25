using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using AzureSearch_WebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureSearch_WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(SearchData model)
        {
            try
            {
                if (model.SearchText == null)
                {
                    model.SearchText = "";
                }
                await RunQueryAsync(model).ConfigureAwait(false);
            }
            catch
            {
                //return View("Error", new ErrorViewModel { RequestId = "1" });
            }
            return View(model);
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequesId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}

        private static SearchClient _searchClient;
        private static SearchIndexClient _indexClient;
        private static IConfigurationBuilder _builder;
        private static IConfigurationRoot _configuration;

        private void InitSearch()
        {
            //creation of the configuration needed using the appsettings file data
            _builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _configuration = _builder.Build();

            //getting the keys that are relevant to the specific functions, with the search service being the main at the moment
            string searchServiceUri = _configuration["SearchServiceUri"];
            string queryApiKey = _configuration["SearchServiceQueryApiKey"];

            _indexClient = new SearchIndexClient(new Uri(searchServiceUri), new AzureKeyCredential(queryApiKey));
            _searchClient = _indexClient.GetSearchClient("jfkindex");
        }

        public async Task<IActionResult> RunQueryAsync(SearchData model)
        {
            InitSearch();

            var options = new SearchOptions()
            {
                IncludeTotalCount = true,
            };

            options.Select.Add("filename");
            options.Select.Add("text");

            //model.resultList = await _searchClient.SearchAsync<SearchData>(model.SearchText, options).ConfigureAwait(false);

            return View("Index", model);
        }
    }
}
