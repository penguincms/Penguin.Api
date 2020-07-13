using Penguin.SystemExtensions.Abstractions.Interfaces;

namespace Penguin.Api.Binary
{
    public class BinaryPostBody : IConvertible<string>
    {
        public byte[] Value { get; set; }

        public string Convert() => System.Convert.ToBase64String(this.Value);

        public void Convert(string fromT) => this.Value = System.Convert.FromBase64String(fromT);
    }
}