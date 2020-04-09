using Penguin.Api.Shared;

namespace Penguin.Api.Binary
{
    public class BinaryPostItem : ApiServerPost<BinaryPostPayload, GenericResponsePayload>
    {
        public override void FillBody(string source)
        {
            this.Request = this.Request ?? new BinaryPostPayload();
            this.Request.Body = new BinaryPostBody();
            this.Request.Body.Convert(source);
        }
    }
}