using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Augong.Framework.Shared
{
    public class SomeHelper
    {

        public string DoSomething(object value)
        {
            var c = JsonConvert.SerializeObject(value);

            return c;
        }
    }
}
