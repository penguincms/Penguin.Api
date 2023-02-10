using Penguin.Api.Shared;

namespace Penguin.Api.Forms
{
    public class FormPostPayload : ServerPostPayload<FormItemCollection>
    {
        public FormPostPayload()
        {
            Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        }

        public override void SetValue(string path, object Value, string newPropName)
        {
            if (newPropName != null)
            {
                Body.Remove(path);
                Body.Add(newPropName, Value?.ToString());
            }
            else
            {
                Body[path] = Value?.ToString();
            }
        }

        public override bool TryGetValue(string path, out object value)
        {
            if (!base.TryGetValue(path, out value))
            {
                if (!Body.ContainsKey(path))
                {
                    value = null;
                    return false;
                }
                else
                {
                    value = Body[path];
                }
            }

            return true;
        }
    }
}