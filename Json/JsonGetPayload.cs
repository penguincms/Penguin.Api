using Penguin.Api.Shared;

namespace Penguin.Api.Json
{
    public class JsonGetPayload : EmptyPayload
    {
        public JsonGetPayload()
        {
            Headers.Add("Accept", "application/json, text/plain, */*");
            Headers.Add("Content-Type", "application/json;charset=UTF-8");
        }
    }
}