using DimplowTools.Commands;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Models
{
    internal class GraphShapeModel : ObservableClass
    {
        private static GraphShapeModel _graphShapeModel;
        private GraphShapeModel()
        { }
        public static GraphShapeModel GetInstance()
        { 
            if (_graphShapeModel==null)
                _graphShapeModel = new GraphShapeModel();
            return _graphShapeModel;
        }

        BidirectionalGraph<Vertex, STaggedEdge<Vertex, int>> _graph = new BidirectionalGraph<Vertex, STaggedEdge<Vertex, int>>();
        public BidirectionalGraph<Vertex, STaggedEdge<Vertex, int>> Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        private int[][] _field;
        public void CreateField(int size)
        {
            int index;
            _field = new int[size][];
            for (index = 0; index < size; index++)
                _field[index] = new int[size];
        }
        public void GenerateVertices(int amount, int minRadius, int maxRadius, int size = 10000)
        {
            int index;
            Random random = new Random();
            if (_field == null)
                CreateField(size);
            int radius, x, y;
            for (index = 0; index < amount; index++)
            { 
                radius = random.Next(minRadius, maxRadius);
                do
                {
                    x = random.Next(_field.Length);
                    y = random.Next(_field.Length);
                }
                while (_field[x][y] == 1);
                _graph.AddVertex(new Vertex(radius, x, y, index));
                OnPropertyChanged();
            }
            OnPropertyChanged();
        }

        public void GenerateEdges()
        {
            List<Vertex> vertices = _graph.Vertices.ToList();
            int index1, index2;
            for (index1 = 0; index1 < vertices.Count(); index1++)
            {
                for (index2 = 0; index2 < vertices.Count(); index2++)
                {
                    if (index1 == index2)
                        continue;
                    if (vertices[index1].X + vertices[index1].Radius > vertices[index2].X &&
                        vertices[index1].X - vertices[index1].Radius < vertices[index2].X &&
                        vertices[index1].Y + vertices[index1].Radius > vertices[index2].Y &&
                        vertices[index1].Y - vertices[index1].Radius < vertices[index2].Y)
                    {
                        _graph.AddEdge(new STaggedEdge<Vertex, int>(vertices[index1], vertices[index2], vertices[index1].Radius));
                        OnPropertyChanged();
                    }
                }
            }
            OnPropertyChanged();
        }
    }
}
