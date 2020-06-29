using Penguin.Api.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Api.Json
{
    public class JsonValueEquals : IExecutionCondition
    {
        public StringComparison ComparisonType { get; set; } = StringComparison.Ordinal;
        public string SourcePath { get; set; }
        public string SourceValue { get; set; }
        public bool ShouldExecute(IApiPlaylistSessionContainer Container)
        {
            if (Container is null)
            {
                throw new ArgumentNullException(nameof(Container));
            }

            (string sourceId, string sourcePath) = JsonTransformation.SplitPath(SourcePath);

            if (Container.Interactions[sourceId].Response.TryGetValue(sourcePath, out object value) && string.Equals(value.ToString(), SourceValue, ComparisonType))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
