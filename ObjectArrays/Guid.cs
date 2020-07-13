namespace Penguin.Api.ObjectArrays
{
    public class Guid : ObjectArray
    {
        public override string GetNew(string index) => System.Guid.NewGuid().ToString();
    }
}