using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using AIO.Operations;
using AIO.ACES.Models;

namespace  workflow_input_variations_autocad.io
{
    class Credentials
    {
        //get your ConsumerKey/ConsumerSecret at http://developer.autodesk.com
        public static string ConsumerKey = "";
        public static string ConsumerSecret = "";
    }
    class Program
    {
        static Container container = null;
        static void Main(string[] args)
        {
            //instruct client side library to insert token as Authorization value into each request
            container = new Container(new Uri("https://developer.api.autodesk.com/autocad.io/us-east/v2/"));
            var token = GetToken();
            container.SendingRequest2 += (sender, e) => e.RequestMessage.SetHeader("Authorization", token);

            //single file without xrefs
            SubmitWorkitem("https://github.com/Developer-Autodesk/library-sample-autocad.io/blob/master/A-01.dwg?raw=true", ResourceKind.Simple);

            //file with xrefs using inline json syntax
            dynamic files = new ExpandoObject();
            files.Resource = "https://github.com/Developer-Autodesk/library-sample-autocad.io/blob/master/A-01.dwg?raw=true";
            files.LocalFileName = "A-01.dwg";
            files.RelatedFiles = new ExpandoObject[2];
            files.RelatedFiles[0] = new ExpandoObject();
            files.RelatedFiles[0].Resource = "https://github.com/Developer-Autodesk/library-sample-autocad.io/blob/master/Res/Grid%20Plan.dwg?raw=true";
            files.RelatedFiles[0].LocalFileName = "Grid Plan.dwg";
            files.RelatedFiles[1] = new ExpandoObject();
            files.RelatedFiles[1].Resource = "https://github.com/Developer-Autodesk/library-sample-autocad.io/blob/master/Res/Wall%20Base.dwg?raw=true";
            files.RelatedFiles[1].LocalFileName = "Wall Base.dwg";
            var json = JsonConvert.SerializeObject(files);
            SubmitWorkitem(json, ResourceKind.RemoteFileResource);

            //etransmit package
            SubmitWorkitem("https://github.com/Developer-Autodesk/library-sample-autocad.io/blob/master/A-01.zip?raw=true", ResourceKind.EtransmitPackage);

            //output to Azure using new Headers property
            SubmitWorkItemWithOutputHeaders();

        }

        static string GetToken()
        {
            using (var client = new HttpClient())
            {
                var values = new List<KeyValuePair<string, string>>();
                values.Add(new KeyValuePair<string, string>("client_id", Credentials.ConsumerKey));
                values.Add(new KeyValuePair<string, string>("client_secret", Credentials.ConsumerSecret));
                values.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                var requestContent = new FormUrlEncodedContent(values);
                var response = client.PostAsync("https://developer.api.autodesk.com/authentication/v1/authenticate", requestContent).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var resValues = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);
                return resValues["token_type"] + " " + resValues["access_token"];
            }
        }
        static void DownloadToDocs(string url, string localFile)
        {
            if (url == null)
                return;
            var client = new HttpClient();
            var content = (StreamContent)client.GetAsync(url).Result.Content;
            var fname = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), localFile);
            Console.WriteLine("Downloading to {0}.", fname);
            using (var output = System.IO.File.Create(fname))
            {
                content.ReadAsStreamAsync().Result.CopyTo(output);
                output.Close();
            }
        }
        static void SubmitWorkitem(string inResource, ResourceKind inResourceKind)
        {
            //create a workitem
            var wi = new WorkItem()
            {
                Id = "", //must be set to empty
                Arguments = new Arguments(),
                ActivityId = "PlotToPDF" //PlotToPDF is a predefined activity
            };

            wi.Arguments.InputArguments.Add(new Argument()
            {
                Name = "HostDwg",// Must match the input parameter in activity
                Resource = inResource,
                ResourceKind = inResourceKind,
                StorageProvider = StorageProvider.Generic //Generic HTTP download (as opposed to A360)
            });
            wi.Arguments.OutputArguments.Add(new Argument()
            {
                Name = "Result", //must match the output parameter in activity
                StorageProvider = StorageProvider.Generic, //Generic HTTP upload (as opposed to A360)
                HttpVerb = HttpVerbType.POST, //use HTTP POST when delivering result
                Resource = null //use storage provided by AutoCAD.IO
            });

            container.AddToWorkItems(wi);
            Console.WriteLine("Submitting workitem...");
            container.SaveChanges();

            pollWorkitem(wi, inResourceKind.ToString());

        }

        // To upload to Azure blob, you are required to specify a value in header "x-ms-blob-type".
        // We added a new Headers property on Argument so that you can set necessary header values for
        // both InputArgument and OutputArgument.
        static void SubmitWorkItemWithOutputHeaders()
        {
            //create a workitem
            var wi = new WorkItem()
            {
                Id = "", //must be set to empty
                Arguments = new Arguments(),
                ActivityId = "PlotToPDF" //PlotToPDF is a predefined activity
            };

            wi.Arguments.InputArguments.Add(new Argument()
            {
                Name = "HostDwg",// Must match the input parameter in activity
                Resource = "https://github.com/Developer-Autodesk/library-sample-autocad.io/blob/master/A-01.dwg?raw=true",
                ResourceKind = ResourceKind.Simple,
                StorageProvider = StorageProvider.Generic //Generic HTTP download (as opposed to A360)
            });

            var outputArgument = new Argument()
            {
                Name = "Result", //must match the output parameter in activity
                StorageProvider = StorageProvider.Generic, //Generic HTTP upload (as opposed to A360)
                HttpVerb = HttpVerbType.PUT, //use HTTP POST when delivering result
                Resource = "http://portalvhdsz7vs58j0h10tp.blob.core.windows.net/test/A-01.pdf?sv=2014-02-14&sr=c&sig=ngiVjMtuQNOWKRZtwosL4va3M7fgg9bt22e6FtH6gEo%3D&st=2015-05-15T07%3A00%3A00Z&se=2018-05-23T07%3A00%3A00Z&sp=rw",
                Headers = new System.Collections.ObjectModel.ObservableCollection<Header>() 
                { 
                    new Header() { Name = "x-ms-blob-type", Value = "BlockBlob" } // This is required for Azure blob.
                }
            };
            wi.Arguments.OutputArguments.Add(outputArgument);

            container.AddToWorkItems(wi);
            Console.WriteLine("Submitting workitem...");
            container.SaveChanges();

            pollWorkitem(wi, "TestOutputHeaders");
        }

        static void pollWorkitem(WorkItem wi, string prefix)
        {
            //polling loop
            do
            {
                Console.WriteLine("Sleeping for 2 sec...");
                System.Threading.Thread.Sleep(2000);
                container.LoadProperty(wi, "Status"); //http request is made here
                Console.WriteLine("WorkItem status: {0}", wi.Status);
            }
            while (wi.Status == ExecutionStatus.Pending || wi.Status == ExecutionStatus.InProgress);

            //re-query the service so that we can look at the details provided by the service
            container.MergeOption = Microsoft.OData.Client.MergeOption.OverwriteChanges;
            wi = container.WorkItems.ByKey(wi.Id).GetValue();

            //Resource property of the output argument "Result" will have the output url
            var url = wi.Arguments.OutputArguments.First(a => a.Name == "Result").Resource;
            DownloadToDocs(url, prefix + "-AIO.pdf");

            //download the status report
            url = wi.StatusDetails.Report;
            DownloadToDocs(url, prefix + "-AIO-report.txt");
        }
    }
}
