using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.Client;

namespace ConsoleApp1
{
    class Program
    {

        private static string? iterationPath;

        // Organization URL, for example: https://dev.azure.com/fabrikam or http://localhost/DefaultCollection
        private static string organizationUrl = "http://localhost/DefaultCollection";
        private static string projectName = "prj01";
        private static string username = "nick";
        private static string pwd = "121518!a";
        static void Main(string[] args)
        {
            iterationPath = "prj01\\Iteration 1";
            //for each parent will be created 1 child and for this child also one more child(Parent->Child->Child)
            //so, if you set quantityOfNewParentWorkItems to 10, total wi quantity will be 30
            //(10 parents -> 10 children -> 10 children)
            int? quantityOfNewParentWorkItems = 5000;
            int? quantityTotalNewWorkItems = 2000;

            CreateWorkItems(organizationUrl, projectName, username, pwd, null, quantityTotalNewWorkItems);

            Console.WriteLine("DONE");
            Console.ReadLine();
        }


        public static void CreateWorkItems(string orgUrl, string projectName, string username, string pwd, int? quantityParents, int? quantityTotal)
        {
            NetworkCredential networkCredential = new NetworkCredential(username, pwd);
            WindowsCredential winCred = new WindowsCredential(networkCredential);
            VssCredentials vssCred = new VssClientCredentials(winCred);
            VssConnection connection = new VssConnection(new Uri(orgUrl), vssCred);
            WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

            for (int i = 0; i < (quantityParents ?? quantityTotal ?? 0); i++)
            {
                JsonPatchDocument patchDocument = getParentPatch();
                WorkItem result = workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, projectName, "Epic").Result;
                string url = result.Url;
                showMessage(i, null, quantityTotal);
                for (int j = 0; j < 1; j++)
                {
                    JsonPatchDocument childDocument = getChildPatch(url);
                    WorkItem result1 = workItemTrackingHttpClient.CreateWorkItemAsync(childDocument, projectName, "Feature").Result;
                    string url1 = result1.Url;
                    if (quantityTotal!=null) i++;
                    showMessage(i, null, quantityTotal);
                    for (int k = 0; k < 1; k++)
                    {
                        JsonPatchDocument childDocument1 = getChildPatch(url1);
                        WorkItem result2 = workItemTrackingHttpClient.CreateWorkItemAsync(childDocument1, projectName, "User Story").Result;
                        string url2 = result2.Url;
                        showMessage(i, null, quantityTotal);
                        if (quantityTotal!=null) i++;
                        for (int r = 0; r < 1; r++)
                        {
                            JsonPatchDocument childDocument2 = getChildPatch(url2);
                            WorkItem result3 = workItemTrackingHttpClient.CreateWorkItemAsync(childDocument2, projectName, "Bug").Result;
                            if (quantityTotal!=null) i++;
                            showMessage(i, null, quantityTotal);
                        }
                    }
                }
                showMessage(i, quantityParents, null);
            }
        }

        private static void showMessage(int i, int? quantityParents, int? quantityTotal)
        {
            if (quantityParents!=null)
            {
                Console.WriteLine($"Parent {(i+1)} of {quantityParents} created - {calculatePercent(quantityParents ?? 0, i+1):P}");
            }
            if (quantityTotal!=null)
            {
                Console.WriteLine($"Work item {(i+1)} of {quantityTotal} created - {calculatePercent(quantityTotal ?? 0, i+1):P}");
            }

        }

        private static decimal calculatePercent(int total, int current)
        {
            return ((decimal)current / (decimal)total);
        }

        private static JsonPatchDocument getParentPatch()
        {
            JsonPatchDocument patchDocument = new JsonPatchDocument();
            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = Guid.NewGuid().ToString()
                }
            );

            return patchDocument;
        }

        private static JsonPatchDocument getChildPatch(string link)
        {
            string title = Guid.NewGuid().ToString();
            string description = "This is a new work item that has a link also created on it.";
            string linkUrl = link;

            JsonPatchDocument patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = title
                },

                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Description",
                    Value = description
                },

                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = linkUrl,
                        attributes = new
                        {
                            comment = "decomposition of work"
                        }
                    }
                }
            };


            if (iterationPath!=null)
            {
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.IterationPath",
                    Value = iterationPath
                });
            }

                return patchDocument;
        }
    }
}
