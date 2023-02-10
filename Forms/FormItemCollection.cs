using Penguin.SystemExtensions.Abstractions.Interfaces;
using Penguin.SystemExtensions.Collections;
using System;
using System.Linq;
using System.Text;

namespace Penguin.Api.Forms
{
    public class FormItemCollection : IListCollection<FormItem>, IConvertible<string>
    {
        public string BadString { get; set; }

        public FormItemCollection(string postString)
        {
            FromString(postString);
        }

        public FormItemCollection()
        {
        }

        public string this[string key]
        {
            get => BackingList.Single(h => h.Name == key).Value;
            set => FindOrCreate(key).Value = value;
        }

        public static FormItemCollection Parse(string toParse)
        {
            return new FormItemCollection(toParse);
        }

        public void Add(string name, string value)
        {
            Add(new FormItem()
            {
                Name = name,
                Value = value
            });
        }

        public bool ContainsKey(string key)
        {
            return this.Any(fi => fi.Name == key);
        }

        string IConvertible<string>.Convert()
        {
            return ToString();
        }

        void IConvertible<string>.Convert(string fromT)
        {
            FromString(fromT);
        }

        public void FromString(string collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            BackingList.Clear();

            collection = collection.TrimEnd('&');

            if (collection.Count(c => c == '&') != collection.Count(c => c == '=') - 1)
            {
                BadString = collection;
                return;
            }

            foreach (string vals in collection.Split('&'))
            {
                if (string.IsNullOrWhiteSpace(vals))
                {
                    continue;
                }

                string Key = vals.Split('=')[0];
                string Value = vals.Split('=')[1];

                BackingList.Add(new FormItem()
                {
                    Name = Uri.UnescapeDataString(Key),
                    Value = Uri.UnescapeDataString(Value)
                });
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(BadString) || !BackingList.Any())
            {
                return BadString;
            }

            StringBuilder sb = new();

            bool first = true;
            foreach (FormItem thisValue in BackingList)
            {
                if (!first)
                {
                    _ = sb.Append('&');
                }

                _ = sb.Append(Uri.EscapeDataString(thisValue.Name));
                _ = sb.Append('=');
                _ = sb.Append(Uri.EscapeDataString(thisValue.Value));

                first = false;
            }

            return sb.ToString();
        }

        private FormItem FindOrCreate(string key)
        {
            FormItem val = BackingList.SingleOrDefault(h => h.Name == key);

            if (val is null)
            {
                val = new FormItem()
                {
                    Name = key
                };

                BackingList.Add(val);
            }

            return val;
        }
    }
}