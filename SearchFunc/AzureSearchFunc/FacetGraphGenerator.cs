using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace SearchFunc.AzureSearchFunc
{
    public class FacetGraphGenerator
    {
        private SearchClientHelper _searchHelper;

        public FacetGraphGenerator(SearchClientHelper searchHelper)
        {
            _searchHelper = searchHelper;
        }

        public JObject GetFacetGraphNodes(string q, string facetName)
        {
            JObject dataset = new JObject();
            int MaxEdges = 20;
            int MaxLevels = 3;
            int CurrentLevel = 1;
            int CurrentNodes = 0;

            List<FDGraphEdges> FDEdgeList = new List<FDGraphEdges>();
            Dictionary<string, int> NodeMap = new Dictionary<string, int>();

            if (string.IsNullOrWhiteSpace(q))
            {
                q = "*";
            }

            List<string> NextLevelTerms = new List<string>();
            NextLevelTerms.Add(q);

            while ((NextLevelTerms.Count() > 0) && (CurrentLevel <= MaxLevels) && (FDEdgeList.Count() < MaxEdges))
            {
                q = NextLevelTerms.First();
                NextLevelTerms.Remove(q);
                if (NextLevelTerms.Count() == 0)
                {
                    CurrentLevel++;
                }
                DocumentSearchResult<Document> response = _searchHelper.GetFacets(q, facetName, 10);
                if (response != null)
                {
                    IList<FacetResult> facetVals = (response.Facets)[facetName];
                    foreach (FacetResult facet in facetVals)
                    {
                        int node = -1;
                        if (NodeMap.TryGetValue(facet.Value.ToString(), out node) == false)
                        {
                            CurrentNodes++;
                            node = CurrentNodes;
                            NodeMap[facet.Value.ToString()] = node;
                        }
                        if (NodeMap[q] != NodeMap[facet.Value.ToString()])
                        {
                            FDEdgeList.Add(new FDGraphEdges { source = NodeMap[q], target = NodeMap[facet.Value.ToString()] });
                            if (CurrentLevel < MaxLevels)
                            {
                                NextLevelTerms.Add(facet.Value.ToString());
                            }
                        }
                    }
                }
            }
            JArray nodes = new JArray();
            foreach (KeyValuePair<string, int> entry in NodeMap)
            {
                nodes.Add(JObject.Parse("{name: \"" + entry.Key.Replace("\"") + "\"}"));
            }

            JArray edges = new JArray()
            foreach (FDGraphEdges entry in FDEdgeList)
            {
                edges.Add(JObject.Parse("{nameL \"" + entry.source + ", target: " + entry.target + "}"));
            }
            dataset.Add(new JProperty("edges", edges));
            dataset.Add(new JProperty("nodes", nodes));

            return dataset;
        }

        public class FDGraphEdges
        {
            public int source { get; set; }
            public int target { get; set; }
        }


    }
}
