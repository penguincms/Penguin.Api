using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Api.Shared
{
    public class EmptyPostItem : ApiServerPost<EmptyPayload, GenericResponsePayload>
    {
        public override void FillBody(string source)
        {

        }
    }
}
