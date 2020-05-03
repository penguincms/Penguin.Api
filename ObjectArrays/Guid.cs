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