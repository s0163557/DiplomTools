using DimplowTools.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DimplowTools.Models
{
    internal class Vertex : ObservableClass
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
                OnPropertyChanged("Radius");
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
                OnPropertyChanged("X");
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
                OnPropertyChanged("Y");
            }
        }
        //Мне кажется, хранить целиком Vertex это слишком дорого
        //поэтому будем хранить там номера вершин, соответствующие номеру в массиве.
        public List<int> NeighborsNumbers;

        public Vertex(int radius, int x, int y)
        {
            Radius = radius;
            X = x;
            Y = y;
        }

        public Thickness PositionOnCanvas
        {
            get
            {
                return new Thickness(X, Y, 0, 0);
            }
        }
    }
}
