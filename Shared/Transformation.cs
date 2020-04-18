using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Api.Shared
{
    public class Transformation
    {
        public Transformation(string value)
        {
            if(value.StartsWith("?"))
            {
                Required = false;
                Value = value.Substring(1);
            } else
            {
                Required = true;
                Value = value;
            }
        }
        public string Value { get; set; }
        public bool Required { get; set; }
    }
}
