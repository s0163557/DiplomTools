using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Tools
{
    internal class Pair<T, R>
    {
        public T First { get; set; }
        public R Second { get; set; }
        public Pair(T first, R second) 
        {
            this.First = first;
            this.Second = second;
        }
    }
}
