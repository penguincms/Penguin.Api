using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Api.ObjectArrays
{
    public class Guid : ObjectArray
    {
        public override string GetNew(string index)
        {
            return System.Guid.NewGuid().ToString();
        }
    }
}
