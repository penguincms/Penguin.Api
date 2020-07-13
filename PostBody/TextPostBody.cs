using Penguin.SystemExtensions.Abstractions.Interfaces;

namespace Penguin.Api.PostBody
{
    public class TextPostBody : IConvertible<string>
    {
        public string Value { get; set; }

        public TextPostBody(string value) => this.Value = value;

        public TextPostBody()
        {
        }

        public static implicit operator string(TextPostBody d) => d?.Value;

        public static implicit operator TextPostBody(string b) => new TextPostBody(b);

        public string Convert() => this.Value;

        public void Convert(string fromT) => this.Value = fromT;

        public override string ToString() => this.Value;
    }
}