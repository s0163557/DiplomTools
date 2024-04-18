using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Models
{
    internal class Vertex
    {
        private int _radius;
        public int Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
            }
        }
        private int _x;
        public int X
        {
            get
            {
                return _x;
            }
            set
            {
                _x = value;
            }
        }
        private int _y;
        public int Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
            }
        }
        public Vertex(int radius, int x, int y, int id)
        {
            Radius = radius;
            X = x;
            Y = y;
            ID = id;
        }

        public string Label;
        public int ID { get; }

        public override string ToString()
        {
            return ID.ToString();
        }
    }
}
