using DimplowTools.Commands;
using GraphShape.Utils;
using QuikGraph;
using QuikGraph.Algorithms;
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
            if (_graphShapeModel == null)
                _graphShapeModel = new GraphShapeModel();
            return _graphShapeModel;
        }

        BidirectionalGraph<Vertex, SEdge<Vertex>> _graph;
        public BidirectionalGraph<Vertex, SEdge<Vertex>> Graph
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
            _graph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
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
            }
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
                        _graph.AddEdge(new SEdge<Vertex>(vertices[index1], vertices[index2]));
                }
            }
            OnPropertyChanged();
        }

        public int FindSingletoneCut()
        {
            List<Vertex> vertices = _graph.Vertices.ToList();
            int minDegree = int.MaxValue;
            for (int i = 0; i < vertices.Count(); i++)
                if (_graph.Degree(vertices[i]) < minDegree)
                    minDegree = _graph.Degree(vertices[i]);
            return minDegree;
        }

        public int Ki(int i)
        {
            return (int)Math.Pow(2, i);
        }

        public int FindRootCut(int Ki0, Vertex rootVertex)
        {
            BidirectionalGraph<Vertex, SEdge<Vertex>> subGraph = _graph.Clone();
            List<Vertex> vertices = _graph.Vertices.ToList();
            int i, j;
            for (i = 0; i < subGraph.VertexCount; i++)
            {
                if (subGraph.InDegree(vertices[i]) >= 2 * Ki0 && rootVertex != vertices[i])
                {
                    List<SEdge<Vertex>> inEdges = subGraph.InEdges(vertices[i]).ToList();
                    for (j = 0; j < inEdges.Count; j++)
                        subGraph.AddEdge(new SEdge<Vertex>(inEdges[j].Source, rootVertex));

                    List<SEdge<Vertex>> outEdges = subGraph.OutEdges(vertices[i]).ToList();
                    for (j = 0; j < outEdges.Count; j++)
                        subGraph.AddEdge(new SEdge<Vertex>(rootVertex, outEdges[j].Target));

                    subGraph.RemoveVertex(vertices[i]);
                }
            }
            _graph = subGraph.Clone();
            OnPropertyChanged();
            return 10;
        }

        public List<int> FindSinkCuts()
        {
            int l = (int)Math.Truncate(Math.Sqrt(_graph.VertexCount));
            int i0 = (int)Math.Log10(l);
            int Ki0 = Ki(i0);
            //Переберём все вершины, сразу авансом для функции связности
            List<int> resultCuts = new List<int>();
            List<Vertex> vertices = _graph.Vertices.ToList();
            FindRootCut(Ki0, vertices[0]); 

            return resultCuts;
        }
    }
}
