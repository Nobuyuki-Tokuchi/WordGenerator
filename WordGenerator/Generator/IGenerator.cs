using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGenerator.Generator
{
    public interface IGenerator
    {
        IDictionary<string, object> Settings { get; set; }

        string Create();
    }
}
