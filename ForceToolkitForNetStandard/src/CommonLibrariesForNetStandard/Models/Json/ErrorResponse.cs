using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Salesforce.Common.Models.Json
{
    public class ErrorResponse
    {
        [JsonProperty(PropertyName = "message")]
        public string Message;
        
        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode;
    }
}
