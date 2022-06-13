using Penguin.Api.Abstractions.ObjectArrays;
using Penguin.Extensions.String;
using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.ObjectArrays
{
    public class ObjectArray : IObjectArray
    {
        private readonly Dictionary<string, string> backingDictionary = new Dictionary<string, string>();

        private readonly Dictionary<string, IObjectArray> initializedArrays = new Dictionary<string, IObjectArray>();

        public string this[string index]
        {
            get
            {
                if (!this.backingDictionary.TryGetValue(index, out string v))
                {
                    v = this.GetNew(index);
                    this.backingDictionary.Add(index, v);
                }

                return v;
            }
        }

        public string Get(string toGet)
        {
            string TypeName = toGet.To("[");
            string index = toGet.From("[").To("]");

            Type arrayType = this.GetType(TypeName);

            if (!this.initializedArrays.TryGetValue(TypeName, out IObjectArray o))
            {
                o = Activator.CreateInstance(arrayType) as IObjectArray;

                this.initializedArrays.Add(TypeName, o);
            }

            return o[index];
        }

        public virtual string GetNew(string index) => throw new NotImplementedException();

        public Type GetType(string TypeName)
        {
            if (this.initializedArrays.TryGetValue(TypeName, out IObjectArray array))
            {
                return array.GetType();
            }
            else
            {
                Type arrayType = TypeFactory.GetAllImplementations<IObjectArray>().Single(t => t.Name == TypeName);

                return arrayType;
            }
        }
    }
}