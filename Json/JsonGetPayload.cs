using Penguin.Api.Shared;

namespace Penguin.Api.Json
{
    public class JsonGetPayload : EmptyPayload
    {
        public JsonGetPayload()
        {
            this.Headers.Add("Accept", "application/json, text/plain, */*");
            this.Headers.Add("Content-Type", "application/json;charset=UTF-8");
        }
    }
}