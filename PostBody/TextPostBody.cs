using Penguin.SystemExtensions.Abstractions.Interfaces;

namespace Penguin.Api.PostBody
{
    public class TextPostBody : IConvertible<string>
    {
        public string Value { get; set; }

        public TextPostBody(string value)
        {
            Value = value;
        }

        public TextPostBody()
        {
        }

        public static implicit operator string(TextPostBody d)
        {
            return d?.Value;
        }

        public static implicit operator TextPostBody(string b)
        {
            return new TextPostBody(b);
        }

        public string Convert()
        {
            return Value;
        }

        public void Convert(string fromT)
        {
            Value = fromT;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}