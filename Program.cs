using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MyPrintNode
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Program().run();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("error connecting to server");
                Console.Error.WriteLine(e.Message + "\n\n");
                Console.Error.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }

        private void run()
        {
            // instance of api client
            PrintNodeService service = new PrintNodeService("https://api.printnode.com/", "<api username here>", "<api password here");

            // submits a print job and prints out the printjob ID
            Console.WriteLine("Print Job ID is " + service.SubmitPrintJob());

            // gets a list of all submitted print jobs
            Console.WriteLine("Below is list of Print Jobs in json:");
            Console.WriteLine(service.GetPrintJobs());

            // gets a list of all computers registered with PrintNode account
            Console.WriteLine("\n\n\n\n Below is list of Computers: ");
            Console.WriteLine(service.GetComputers());

            // gets a list of all printers registered with PrintNode account
            Console.WriteLine("\n\n\n\n Below is list of Printers: ");
            Console.WriteLine(service.GetPrinters());

            // haults program from exiting
            Console.ReadKey();
        }
    }

    // PrintNodeService.cs
    public class PrintNodeService
    {
        private readonly string address;
        private readonly string username;
        private readonly string password;

        // TODO send base64 image
        // TODO remove this
        private readonly Dictionary<string, string> Endpoints = new Dictionary<string, string>()
			{
				{"Computers", "computers.json"},
				{"Printers", "printers.json"},
				{"PrintJobs", "printjobs.json"}
			};

        public PrintNodeService(string address, string username, string password)
        {
            this.address = address;
            this.username = username;
            this.password = password;
        }

        public string GetComputers()
        {
            return Get("Computers");
        }

        public string GetPrinters()
        {
            return Get("Printers");
        }

        public string GetPrintJobs()
        {
            return Get("PrintJobs");
        }

        /**
         * @return string  id of printjob
         */
        public string SubmitPrintJob()
        {

            string source = "PrintNodeApi/1.0";
            string title = "YukonTestPdf";
            string printerId = "1";

            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"printer\": " + printerId + ",");
            sb.Append("\"title\": \"" + title + "\",");
            sb.Append("\"contentType\": \"pdf_uri\",");
            sb.Append("\"title\": \"" + title + "\",");
            sb.Append("\"content\": \"" + "http://www.education.gov.yk.ca/pdf/pdf-test.pdf" + "\",");
            sb.Append("\"source\": \"PrintNodeApi/1.0\"");
            sb.Append("}");
            
            string response = Post("PrintJobs", sb.ToString());

            // strips off quotes around the id
            if (response != null)
            {
                return response.Substring(1, response.Length - 2);
            }
            else
            {
                return response;
            }
        }

        private Uri BuildUri(string dir)
        {
            if (!Endpoints.ContainsKey(dir))
            {
                throw new Exception("no endpoint defined for " + dir + ". consider adding the endpoint to the Endpoints dictionary defined in PrintNodeService class");
            }
            var url = address + '/' + Endpoints[dir];
            return new Uri(url);
        }

        private HttpWebRequest BuildRequest(string dir, String method)
        {
            var uri = BuildUri(dir);

            ASCIIEncoding encoding = new ASCIIEncoding();
            var request = (HttpWebRequest)WebRequest.Create(uri);
            // headers
            request.Method = method;
            request.ContentType = "application/json";

            // basic authentication
            string credentials = String.Format("{0}:{1}", username, password);
            byte[] bytes = Encoding.ASCII.GetBytes(credentials);
            string base64 = Convert.ToBase64String(bytes);
            string authorization = String.Concat("Basic ", base64);
            request.Headers.Add("Authorization", authorization);

            return request;
        }

        private string getResponseBody(HttpWebRequest request)
        {
            // gets response
            HttpWebResponse response = null;
            response = (HttpWebResponse)request.GetResponse();
            return new StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        private string Get(string dir)
        {
            var request = BuildRequest(dir, "GET");
            return getResponseBody(request);
        }

        private string Post(string dir, string postData)
        {
            var request = BuildRequest(dir, "POST");

            // data
            var data = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = data.Length;

            // writes data

            var requestStream = request.GetRequestStream();
            requestStream.Write(data, 0, data.Length);
            requestStream.Close();

            return getResponseBody(request);

        }
    }
}
