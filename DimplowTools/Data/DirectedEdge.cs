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
        private int _weight, _occupiedWeight;

        public DirectedEdge(T source, T target, int weight)
        {
            _source = source;
            _target = target;
            _weight = weight;
            _occupiedWeight = 0;
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
        public int Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                _weight = value;
            }
        }
        public int OccupiedWeight
        {
            get
            {
                return _occupiedWeight;
            }
            set
            {
                _occupiedWeight = value;
            }
        }
        public override string ToString()
        {
            return _source.ToString() + "->" + _target.ToString() + "|" + _occupiedWeight + "/" + _weight;
        }
    }
}
