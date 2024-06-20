using DimplowTools.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Data
{
    internal class WeightedVertex : ObservableClass
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

        private int _moveGlobalX = -1;
        public int MoveGlobalX
        {
            get
            {
                return _moveGlobalX;
            }
            set
            {
                _moveGlobalX = value;
            }
        }
        private int _moveGlobalY = -1;
        public int MoveGlobalY
        {
            get
            {
                return _moveGlobalY;
            }
            set
            {
                _moveGlobalY = value;
            }
        }

        private int _stepSize = -1;
        public int StepSize
        {
            get
            {
                return _stepSize;
            }
            set
            {
                _stepSize = value;
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
        public WeightedVertex(int radius, int x, int y, int id, int signalPower)
        {
            Radius = radius;
            X = x;
            Y = y;
            ID = id;
            _signalPower = signalPower;
        }

        private int _id;

        public int ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private int _signalPower;
        public int SignalPower
        {
            get
            {
                return _signalPower;
            }
            set
            {
                _signalPower = value;
            }
        }

        public override string ToString()
        {
            return ID.ToString() + "/" + SignalPower.ToString();
        }
    }
}
