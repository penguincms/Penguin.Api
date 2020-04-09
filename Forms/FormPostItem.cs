using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;

namespace Penguin.Api.Forms
{
    public class FormPostItem : ApiServerPost<FormPostPayload, GenericResponsePayload>
    {
        public override void FillBody(string source)
        {
            this.Request = this.Request ?? new FormPostPayload();
            this.Request.Body = new FormItemCollection();
            this.Request.Body.FromString(source);
        }

        public override FormPostPayload Transform(IApiPlaylistSessionContainer Container)
        {
            FormPostPayload clonedRequest = base.Transform(Container);

            foreach (FormItem fi in clonedRequest.Body)
            {
                if (fi.Name[0] != '$')
                {
                    continue;
                }

                fi.Name = fi.Name.Substring(1);
                fi.Value = base.GetReplacement(fi.Value, Container);
            }

            return clonedRequest;
        }
    }
}