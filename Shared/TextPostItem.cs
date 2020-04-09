using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.PostBody;

namespace Penguin.Api.Shared
{
    public class TextPostItem : ApiServerPost<TextPostPayload, GenericResponsePayload>
    {
        public override void FillBody(string source)
        {
            this.Request = this.Request ?? new TextPostPayload();
            this.Request.Body = new TextPostBody();
            this.Request.Body.Convert(source);
        }

        public override TextPostPayload Transform(IApiPlaylistSessionContainer Container)
        {
            return null;
        }
    }
}