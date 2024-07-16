using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Tools
{
    internal class UndirectedGraphData
    {
        public int AmountOfVertices { get; set; } 
        public int Radius { get; set; } 
        public int FieldSize { get; set; } 
        public int GraphAmount { get; set; }
        public int ID { get; set; }

        public UndirectedGraphData(int amountOfVertives, int radius, int fieldSize, int graphAmount, int id)
        { 
            AmountOfVertices = amountOfVertives;
            Radius = radius;
            FieldSize = fieldSize;
            GraphAmount = graphAmount;
            ID = id;
        }
    }
}
