using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernWordreference.Models
{
    public class JsBridgeResponse<T>
    {
        public bool Done { get; set; }
        public T Object { get; set; }
    }
}
