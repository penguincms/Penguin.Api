﻿using Penguin.Api.PostBody;
using System;

namespace Penguin.Api.Shared
{
    public class TextPostPayload : ServerPostPayload<TextPostBody>
    {
        public TextPostPayload()
        {
            Headers.Add("Accept", "text/plain, */*; q=0.01");
            Headers.Add("Content-Type", "text/plain;charset=UTF-8");
        }

        public override void SetValue(string path, object Value, string newPropName)
        {
            throw new NotImplementedException();
        }
    }
}