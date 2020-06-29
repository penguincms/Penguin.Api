using Penguin.Api.Abstractions.Interfaces;
using Penguin.Api.Shared;
using Penguin.Web.Abstractions.Interfaces;
using System;

namespace Penguin.Api.Forms
{
    public class FormPostItem : BasePostItem<FormPostPayload, GenericResponsePayload>
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
                if (base.TryGetReplacement(fi.Value, Container, out object v))
                {
                    fi.Value = v.ToString();
                }
            }

            return clonedRequest;
        }

        public override bool TryCreate(IHttpServerRequest request, IHttpServerResponse response, out HttpPlaylistItem<FormPostPayload, GenericResponsePayload> item)
        {
            if(TryCreate(request, response, "application/x-www-form-urlencoded", out item))
            {
                item.Request.Body = new FormItemCollection(request.BodyText);

                return true;
            } else
            {
                return false;
            }


        }
    }
}