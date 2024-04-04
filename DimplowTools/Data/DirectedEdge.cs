using DimplowTools.Models;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Data
{
    internal class DirectedEdge<T> : IEdge<T>
    {
        private T _source, _target;
        public DirectedEdge(T source, T target)
        {
            _source = source;
            _target = target;
        }
        public T Source
        {
            get
            {
                return _source;
            }

            set
            {
                _source = value;
            }
        }

        public T Target
        {
            get
            {
                return _target;
            }

            set
            {
                _target = value;
            }
        }

        public bool IsVisited = false;
    }
}
