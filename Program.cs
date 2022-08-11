using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
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
        static void Main(string[] args)
        {
            // Organization URL, for example: https://dev.azure.com/fabrikam or http://localhost/DefaultCollection
            string organizationUrl = "http://localhost/DefaultCollection";
            string projectName = "prj01";
            string username = "your_username";
            string pwd = "blablabla123";
            //for each parent will be created 1 child and for this child also one more child(Parent->Child->Child)
            //so, if you set quantityOfNewParentWorkItems to 10, total wi quantity will be 30
            //(10 parents -> 10 children -> 10 children)
            int quantityOfNewParentWorkItems = 5000;

            CreateWorkItems(organizationUrl, projectName, username, pwd, quantityOfNewParentWorkItems);

            Console.WriteLine("DONE");
            Console.ReadLine();
        }


        public static void CreateWorkItems(string orgUrl, string projectName, string username, string pwd, int quantity)
        {
            NetworkCredential networkCredential = new NetworkCredential(username, pwd);
            WindowsCredential winCred = new WindowsCredential(networkCredential);
            VssCredentials vssCred = new VssClientCredentials(winCred);
            VssConnection connection = new VssConnection(new Uri(orgUrl), vssCred);
            WorkItemTrackingHttpClient workItemTrackingHttpClient = connection.GetClient<WorkItemTrackingHttpClient>();

            for (int i = 0; i < quantity; i++)
            {
                JsonPatchDocument patchDocument = new JsonPatchDocument();
                patchDocument = getParentPatch();
                WorkItem result = workItemTrackingHttpClient.CreateWorkItemAsync(patchDocument, projectName, "User Story").Result;
                string url = result.Url;

                for (int j = 0; j < 1; j++)
                {
                    JsonPatchDocument childDocument = new JsonPatchDocument();
                    childDocument = getChildPatch(url);
                    WorkItem result1 = workItemTrackingHttpClient.CreateWorkItemAsync(childDocument, projectName, "Bug").Result;
                    string url1 = result1.Url;
                    for (int k = 0; k < 1; k++)
                    {
                        JsonPatchDocument childDocument1 = new JsonPatchDocument();
                        childDocument1 = getChildPatch(url1);
                        WorkItem result2 = workItemTrackingHttpClient.CreateWorkItemAsync(childDocument1, projectName, "Task").Result;
                    }
                }

                Console.WriteLine($"Parent created: {(i+1)} - {calculatePercent(quantity, i+1).ToString("P")}");
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

            JsonPatchDocument patchDocument = new JsonPatchDocument();

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = title
                }
            );

            patchDocument.Add(
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Description",
                    Value = description
                }
            );

            patchDocument.Add(
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
            );

            return patchDocument;
        }
    }
}