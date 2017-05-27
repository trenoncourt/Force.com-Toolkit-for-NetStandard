using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Salesforce.Common.FunctionalTests.Models;
using Salesforce.Common.Models.Json;

namespace Salesforce.Common.FunctionalTests
{
	[TestClass]
    public class CommonTests
    {
        public class BulkForceClientConfig
        {
            public string ConsumerKey { get; set; }

            public string ConsumerSecret { get; set; }

            public string UserName { get; set; }

            public string Password { get; set; }

            public string TokenRequestEndpointUrl { get; set; }
        }

        private AuthenticationClient _auth;
	    private JsonHttpClient _jsonHttpClient;
        private BulkForceClientConfig _config;

        [ClassInitialize]
        public void Init()
        {

            string jsonConfig = File.ReadAllText("appsettings.json");
            var config = JsonConvert.DeserializeObject<BulkForceClientConfig>(jsonConfig);
            _config = config;

            _auth = new AuthenticationClient();
            _auth.UsernamePasswordAsync(config.ConsumerKey, config.ConsumerSecret, config.UserName, config.Password, config.TokenRequestEndpointUrl).Wait();
            
            _jsonHttpClient = new JsonHttpClient(_auth.InstanceUrl, _auth.ApiVersion, _auth.AccessToken, new HttpClient());
        }

        [TestMethod]
        public async Task Get_UserInfo()
        {
            var objectName = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("AccessToken", _auth.AccessToken)
                });

            var response = await _jsonHttpClient.HttpGetAsync<UserInfo>(new Uri(_auth.Id));

            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task Query_Describe()
        {
            const string objectName = "Account";
            var response = await _jsonHttpClient.HttpGetAsync<dynamic>(string.Format("sobjects/{0}", objectName));
            
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task Query_Objects()
        {
            var response = await _jsonHttpClient.HttpGetAsync<DescribeGlobalResult<dynamic>>(string.Format("sobjects"));

            Assert.IsTrue(response.MaxBatchSize > 0);
            Assert.IsTrue(response.SObjects.Count > 0);
        }

        [TestMethod]
        public async Task Query_Select_Account()
        {
            const string query = "SELECT Id FROM Account";
            var response = await _jsonHttpClient.HttpGetAsync<QueryResult<dynamic>>(string.Format("query?q={0}", query));

            Assert.IsTrue(response.TotalSize > 0);
            Assert.IsTrue(response.Records.Count > 0);
        }

        [TestMethod]
        public async Task Query_Select_Count()
        {
            const string query = "SELECT count() FROM Account";
            var response = await _jsonHttpClient.HttpGetAsync<QueryResult<dynamic>>(string.Format("query?q={0}", query));

            Assert.IsTrue(response.TotalSize > 0);
            Assert.IsTrue(response.Records.Count == 0);
        }

        [TestMethod]
        public void Auth_UsernamePassword_HasAccessToken()
        {
            Assert.IsNotNull(_auth.AccessToken);
            Assert.AreNotEqual(_auth.AccessToken, string.Empty);
        }

        [TestMethod]
        public void Auth_UsernamePassword_HasInstanceUrl()
        {
            Assert.IsNotNull(_auth.InstanceUrl);
            Assert.AreNotEqual(_auth.InstanceUrl, string.Empty);
        }

        [TestMethod]
        public async Task Auth_InvalidLogin()
        {
            try
            {
                await _auth.UsernamePasswordAsync(_config.ConsumerKey, _config.ConsumerSecret, _config.UserName, "WRONGPASSWORD", _config.TokenRequestEndpointUrl);
            }
            catch (ForceAuthException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsNotNull(ex.Message);
                Assert.IsNotNull(ex.Error);
                Assert.IsNotNull(ex.HttpStatusCode);

                Assert.AreEqual(ex.Message, "authentication failure");
                Assert.AreEqual(ex.Error, Error.InvalidGrant);
                Assert.AreEqual(ex.HttpStatusCode, HttpStatusCode.BadRequest);
            }
        }

	    [TestMethod]
	    public async Task Auth_InvalidRequestEndpointUrl()
	    {
	        const string requestEndpointUrl = "https://login.salesforce.com/services/oauth2/authorizeee"; // typo in the url

	        try
	        {
                await _auth.WebServerAsync("clientId", "clientSecret", "sfdc://success", "code", requestEndpointUrl);
	        }
            catch (ForceAuthException ex)
	        {
	            Assert.IsNotNull(ex);
                
                Assert.AreEqual(ex.Error, Error.UnknownException);
                Assert.AreEqual(ex.Message, "Unexpected character encountered while parsing value: <. Path '', line 0, position 0.");
	        }
	    }

	    [TestMethod]
	    public async Task Upsert_Update_CheckReturn()
	    {
            var account = new Account { Name = "New Account ExternalID", Description = "New Account Description" };
            var response = await _jsonHttpClient.HttpPatchAsync(account, string.Format("sobjects/{0}/{1}/{2}", "Account", "ExternalID__c", "2"));

            Assert.IsNotNull(response);
	    }

        [TestMethod]
        public async Task Upsert_New_CheckReturnInclude()
        {
            var account = new Account { Name = "New Account" + DateTime.Now.Ticks, Description = "New Account Description" + DateTime.Now.Ticks };
            var response = await _jsonHttpClient.HttpPatchAsync(account, string.Format("sobjects/{0}/{1}/{2}", "Account", "ExternalID__c", DateTime.Now.Ticks));

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Id);
        }

	    [TestMethod]
	    public async Task BadTokenHandling()
	    {
	        var badToken = "badtoken";
            var serviceHttpClient = new JsonHttpClient(_auth.InstanceUrl, _auth.ApiVersion, badToken, new HttpClient());

            const string query = "SELECT count() FROM Account";

	        try
	        {
                await serviceHttpClient.HttpGetAsync<QueryResult<dynamic>>(string.Format("query?q={0}", query));
	        }
            catch (ForceException ex)
            {
                Assert.IsNotNull(ex);
                Assert.IsNotNull(ex.Message);
                Assert.AreEqual(ex.Message, "Session expired or invalid");
                Assert.IsNotNull(ex.Error);
            }
	    }

	    [TestMethod]
	    public void CheckInterfaces()
	    {
            using (IAuthenticationClient aa = new AuthenticationClient())
            {
                Assert.IsNotNull(aa);
            }
            using (IJsonHttpClient aa = new JsonHttpClient("instanceUrl", "apiVersion", "accessToken", new HttpClient()))
            {
                Assert.IsNotNull(aa);
            }
            using (IXmlHttpClient aa = new XmlHttpClient("instanceUrl", "apiVersion", "accessToken", new HttpClient()))
            {
                Assert.IsNotNull(aa);
            }
	    }
    }
}
