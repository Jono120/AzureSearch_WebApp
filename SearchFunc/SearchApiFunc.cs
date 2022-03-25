using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using SearchFunc.AzureSearchFunc;

namespace SearchFunc
{
    public static class SearchApiFunc
    {
        [FunctionName("Function1")]
        public static void Run([CosmosDBTrigger(
            databaseName: "pocindex",
            collectionName: "pocdb",
            ConnectionStringSetting = "AccountEndpoint=https://dbpoc.documents.azure.com:443/;AccountKey=EDclwoEVLNOUJLk4DxhGyQ7n7QFAD8G4g5P00XZCExafeVgcBQA8RA3USA8M0qYEwpBD1pkcqGwurbkQOpPo1Q==;",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }

        [FunctionName("facet-graph-nodes")]
        public static IActionResult GetFacetGraphNodes([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, TraceWriter log, ExecutionContext executionContext)
        {
            string skillName = executionContext.FunctionName;
            if (!req.QueryString.HasValue)
            {
                return new BadRequestObjectResult($"{skillName} - Requires a query string in the following format: q=blah&f=blah");
            }

            string searchServiceName = GetAppSetting("SearchServiceName");
            string searchServiceApiKey = GetAppSetting("SearchServiceApiKey");
            string indexName = String.IsNullOrEmpty(req.Headers["IndexName"]) ? Config.AZURE_SEARCH_INDEX_NAME : (string)req.Headers["IndexName"];
            if (String.IsNullOrEmpty(searchServiceName) || String.IsNullOrEmpty(searchServiceApiKey) || String.IsNullOrEmpty(indexName))
            {
                return new BadRequestObjectResult($"{skillName} - Information for the search service is missing");
            }
            SearchClientHelper searchClient = new SearchClientHelper(searchServiceName, searchServiceApiKey, indexName);

            FacetGrapGenerator facetGrapGenerator = new FacetGrapGenerator(searchClient);
            string query = string.IsNullOrEmpty(req.Query["q"].FirstOrDefault()) ? "*" : req.Query["q"].First();
            string facet = string.IsNullOrEmpty(req.Query["f"].FirstOrDefault()) ? "entities" : req.Query["f"].First();
            JObject facetGraph = facetGrapGenerator.GetFacetGraphNodes(query, facet);

            return (ActionResult)new OkObjectResult(facetGraph);

        }


    }
}
