using Loxifi;
using Penguin.Api.Abstractions.Interfaces;
using Penguin.Extensions.String;
using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Api.ObjectArrays
{
    public class ObjectArray : IObjectArray
    {
        private readonly Dictionary<string, string> backingDictionary = new();

        private readonly Dictionary<string, IObjectArray> initializedArrays = new();

        public string this[string index]
        {
            get
            {
                if (!backingDictionary.TryGetValue(index, out string v))
                {
                    v = GetNew(index);
                    backingDictionary.Add(index, v);
                }

                return v;
            }
        }

        public string Get(string index)
        {
            string TypeName = index.To("[");

            index = index.From("[").To("]");

            Type arrayType = GetType(TypeName);

            if (!initializedArrays.TryGetValue(TypeName, out IObjectArray o))
            {
                o = Activator.CreateInstance(arrayType) as IObjectArray;

                initializedArrays.Add(TypeName, o);
            }

            return o[index];
        }

        public virtual string GetNew(string index)
        {
            throw new NotImplementedException();
        }

        public Type GetType(string TypeName)
        {
            if (initializedArrays.TryGetValue(TypeName, out IObjectArray array))
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