using Penguin.Api.Shared;
using System;

namespace Penguin.Api.Binary
{
    public class BinaryPostPayload : ServerPostPayload<BinaryPostBody>
    {
        public BinaryPostPayload()
        {
        }

        public override void SetValue(string path, object Value, string newPropName)
        {
            throw new NotImplementedException();
        }
    }
}