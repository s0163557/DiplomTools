using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Tools
{
    internal class DirectedMoveResearchPointsData
    {
        public int VertexAmount { get; set; }
        public int MinRadius { get; set; }
        public int MaxRadius { get; set; }
        public int MinWeight { get; set; }
        public int MaxWeight { get; set; }
        public int MoveAmount { get; set; }
        public int MinRadiusMovement { get; set; }
        public int MaxRadiusMovement { get; set; }
        public int FieldSize { get; set; }
        public int ID { get; set; }
        public DirectedMoveResearchPointsData(int vertexAmount, int minRadius, int maxRadius, int minWeight, int maxWeight, int moveAmount, int minRadiusMovement, int maxRadiusMovement, int fieldSize, int id)
        {
            VertexAmount = vertexAmount;
            MinRadius = minRadius;
            MaxRadius = maxRadius;
            MinWeight = minWeight;
            MaxWeight = maxWeight;
            MoveAmount = moveAmount;
            MinRadiusMovement = minRadiusMovement;
            MaxRadiusMovement = maxRadiusMovement;
            FieldSize = fieldSize;
            ID = id;
        }
    }
}
