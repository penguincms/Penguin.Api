namespace Penguin.Api.Shared
{
    public class EmptyPayload : ApiPayload
    {
        public override void SetValue(string path, object Value, string newPropName)
        {
            throw new System.NotImplementedException();
        }
    }
}