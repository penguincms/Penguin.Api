using System;

namespace Penguin.Api.Shared
{
    public class Transformation
    {
        public bool Required { get; set; }

        public string Value { get; set; }

        public Transformation(string value)
        {
            if (value is null)
            {
                throw new System.ArgumentNullException(nameof(value));
            }

            if (value.StartsWith("?", StringComparison.OrdinalIgnoreCase))
            {
                Required = false;
                Value = value[1..];
            }
            else
            {
                Required = true;
                Value = value;
            }
        }
    }
}