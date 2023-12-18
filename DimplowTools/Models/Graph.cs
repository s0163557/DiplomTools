using DimplowTools.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Models
{
    internal class Graph : ObservableClass
    {
        private static Graph _graph;
        public Graph() { }
        public static Graph GetInstanse()
        {
            if (_graph == null)
                _graph = new Graph();
            return _graph;
        }
        public ObservableCollection<Vertex> Vertices;
        
        public void GenerateVertices(int heightOfCanvas, int widthOfCanvas, 
            int minRadius, int maxRadius, int numOfVertices)
        {
            int[][] CoordinatesOfCanvas = new int[widthOfCanvas][];
            int index;
            Vertices = new ObservableCollection<Vertex>();
            Random random = new Random();
            for (index = 0; index < CoordinatesOfCanvas.Length; index++)
                CoordinatesOfCanvas[index] = new int[heightOfCanvas];

            int X, Y;
            for (index = 0; index < numOfVertices; index++)
            {
                X = random.Next(0, widthOfCanvas);
                Y = random.Next(0, heightOfCanvas);
                if (CoordinatesOfCanvas[X][Y] == 0)
                {
                    CoordinatesOfCanvas[X][Y] = 1;
                    Vertices.Add(new Vertex(random.Next(minRadius, maxRadius), X, Y));
                }
            }
            OnPropertyChanged();
        }
    }
}
