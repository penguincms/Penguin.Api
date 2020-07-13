namespace Penguin.Api.Shared
{
    public class EmptyResponsePayload : ApiServerResponse
    {
        public override void SetValue(string path, object Value, string newPropName) => throw new System.NotImplementedException();
    }
}