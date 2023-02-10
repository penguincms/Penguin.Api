using Penguin.Api.Shared;
using Penguin.Web.Abstractions.Interfaces;

namespace Penguin.Api.Binary
{
    public class BinaryPostItem : BasePostItem<BinaryPostPayload, GenericResponsePayload>
    {
        public override void FillBody(string source)
        {
            Request ??= new BinaryPostPayload();
            Request.Body = new BinaryPostBody();
            Request.Body.Convert(source);
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<BinaryPostPayload, GenericResponsePayload> item)
        {
            if (request is null)
            {
                throw new System.ArgumentNullException(nameof(request));
            }

            if (request.Method == "POST" && request.ContentType.Contains("binary"))
            {
                item = new BinaryPostItem();
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }
    }
}