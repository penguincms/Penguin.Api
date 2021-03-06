﻿using Penguin.SystemExtensions.Abstractions.Interfaces;
using Penguin.SystemExtensions.Collections;
using System;
using System.Linq;
using System.Text;

namespace Penguin.Api.Forms
{
    public class FormItemCollection : IListCollection<FormItem>, IConvertible<string>
    {
        public string BadString { get; set; }

        public FormItemCollection(string postString) => this.FromString(postString);

        public FormItemCollection()
        {
        }

        public string this[string key]
        {
            get => this.BackingList.Single(h => h.Name == key).Value;
            set => this.FindOrCreate(key).Value = value;
        }

        public static FormItemCollection Parse(string toParse) => new FormItemCollection(toParse);

        public void Add(string name, string value)
        {
            this.Add(new FormItem()
            {
                Name = name,
                Value = value
            });
        }

        public bool ContainsKey(string key) => this.Any(fi => fi.Name == key);

        string IConvertible<string>.Convert() => this.ToString();

        void IConvertible<string>.Convert(string fromT) => this.FromString(fromT);

        public void FromString(string collection)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            this.BackingList.Clear();

            collection = collection.TrimEnd('&');

            if (collection.Count(c => c == '&') != collection.Count(c => c == '=') - 1)
            {
                this.BadString = collection;
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

                this.BackingList.Add(new FormItem()
                {
                    Name = Uri.UnescapeDataString(Key),
                    Value = Uri.UnescapeDataString(Value)
                });
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(this.BadString) || !this.BackingList.Any())
            {
                return this.BadString;
            }

            StringBuilder sb = new StringBuilder();

            bool first = true;
            foreach (FormItem thisValue in this.BackingList)
            {
                if (!first)
                {
                    sb.Append("&");
                }

                sb.Append(Uri.EscapeDataString(thisValue.Name));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(thisValue.Value));

                first = false;
            }

            return sb.ToString();
        }

        private FormItem FindOrCreate(string key)
        {
            FormItem val = this.BackingList.SingleOrDefault(h => h.Name == key);

            if (val is null)
            {
                val = new FormItem()
                {
                    Name = key
                };

                this.BackingList.Add(val);
            }

            return val;
        }
    }
}