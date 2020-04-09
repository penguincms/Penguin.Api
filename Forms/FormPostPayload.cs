using Penguin.Api.Shared;

namespace Penguin.Api.Forms
{
    public class FormPostPayload : ServerPostPayload<FormItemCollection>
    {
        public FormPostPayload()
        {
            this.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            this.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        }

        public override void SetValue(string path, string Value, string newPropName)
        {
            if (newPropName != null)
            {
                this.Body.Remove(path);
                this.Body.Add(newPropName, Value);
            }
            else
            {
                this.Body[path] = Value;
            }
        }

        public override bool TryGetValue(string path, out string value)
        {
            if (!base.TryGetValue(path, out value))
            {
                if (!this.Body.ContainsKey(path))
                {
                    value = null;
                    return false;
                }
                else
                {
                    value = this.Body[path];
                }
            }

            return true;
        }
    }
}