using Penguin.Api.Abstractions.ObjectArrays;
using Penguin.Extensions.Strings;
using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Penguin.Api.ObjectArrays
{
    public class ObjectArray : IObjectArray
    {
        public string Get(string toGet)
        {
            string TypeName = toGet.To("[");
            string index = toGet.From("[").To("]");

            Type arrayType = GetType(TypeName);

            if(!initializedArrays.TryGetValue(TypeName, out IObjectArray o))
            {
                o = Activator.CreateInstance(arrayType) as IObjectArray;

                initializedArrays.Add(TypeName, o);
            }

            return o[index];
        }

        public Type GetType(string TypeName)
        {
            if (initializedArrays.TryGetValue(TypeName, out IObjectArray array))
            {
                return array.GetType();
            } else
            {
                Type arrayType = TypeFactory.GetAllImplementations<IObjectArray>().Single(t => t.Name == TypeName);

                return arrayType;
            }

            
        }
        private Dictionary<string, IObjectArray> initializedArrays = new Dictionary<string, IObjectArray>();

        private Dictionary<string, string> backingDictionary = new Dictionary<string, string>();
        
        public virtual string GetNew(string index)
        {
            throw new NotImplementedException();
        }

        public string this[string index] { 
            get
            {
                if(!backingDictionary.TryGetValue(index, out string v))
                {
                    v = this.GetNew(index);
                    backingDictionary.Add(index, v);
                }

                return v;
            }
        }
    }
}
