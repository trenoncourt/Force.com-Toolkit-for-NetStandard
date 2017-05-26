using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Salesforce.Force.UnitTests.Models;

namespace Salesforce.Force.UnitTests
{
    [TestClass]
    public class ForceClientTests
    {
        private const string UserAgent = "forcedotcom-toolkit-dotnet";
        private const string ApiVersion = "v36";

        [TestMethod]
        public async Task Requests_CheckHttpRequestMessage_UserAgent()
        {
            var httpClient = new HttpClient(new ServiceClientRouteHandler(r => Assert.AreEqual(r.Headers.UserAgent.ToString(), UserAgent + "/v32")));
            var forceClient = new ForceClient("http://localhost:1899", "accessToken", ApiVersion, httpClient, new HttpClient());

           try
           {
               // suppress error; we only care about checking the header
               await forceClient.QueryAsync<object>("query");
           }
           catch
           {
               // do nothing
           }
        }

        [TestMethod]
        public async Task GetBasicInformationAsync_EmptyObjectName_ThrowsException()
        {
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new JsonContent(new { }) };
            var httpClient = new HttpClient(new FakeHttpRequestHandler(expectedResponse));
            var forceClient = new ForceClient("http://localhost:1899", "accessToken", ApiVersion, httpClient, new HttpClient());

            Action<ArgumentNullException> asserts = exception => Assert.IsNotNull(exception.Message);
            await AssertEx.ThrowsAsync(() => forceClient.BasicInformationAsync<object>(""), asserts);
        }

        [TestMethod]
        public async void GetBasicInformationAsync_ValidObjectName_ReturnsParsedResponse()
        {
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.FromFile("KnownGoodContent/UserObjectDescribeMetadata.json")
            };
            var httpClient = new HttpClient(new FakeHttpRequestHandler(expectedResponse));
            var forceClient = new ForceClient("http://localhost:1899", "accessToken", ApiVersion, httpClient, new HttpClient());

            var result = await forceClient.BasicInformationAsync<ObjectDescribeMetadata>("ValidObjectName");

            Assert.IsNotNull(result.Name);
            Assert.AreEqual("User", result.Name);
        }
    }
}
