using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Tools
{
    internal class GraphData
    {
        public int AmountOfVertices;
        public int MinRadius;
        public int MaxRadius;
        public int MinWeight;
        public int MaxWeight;
        public int TaskId;
        public int FieldSize;
        public GraphData(int amountOfVertices, int minRadius, int maxRadius, int minWeight, int maxWeight, int taskId, int fieldSize)
        { 
            AmountOfVertices = amountOfVertices;
            MinRadius = minRadius;
            MaxRadius = maxRadius;
            MinWeight = minWeight;
            MaxWeight = maxWeight;
            TaskId = taskId;
            FieldSize = fieldSize;
        }
    }
}
