using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Tools
{
    internal class MoveResearchPointsData
    {
        public int VertexAmount { get; set; }
        public int Radius { get; set; }
        public int MoveAmount { get; set; }
        public int MinRadiusMovement { get; set; }
        public int MaxRadiusMovement { get; set; }
        public int FieldSize { get; set; }
        public int ID { get; set; }
        public MoveResearchPointsData(int vertexAmount, int radius, int moveAmount, int minRadiusMovement, int maxRadiusMovement, int fieldSize, int id)
        { 
            VertexAmount = vertexAmount;
            Radius = radius;
            MoveAmount = moveAmount;
            MinRadiusMovement = minRadiusMovement;
            MaxRadiusMovement = maxRadiusMovement;
            FieldSize = fieldSize;
            ID = id;
        }
    }
}
