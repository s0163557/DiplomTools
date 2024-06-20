using DimplowTools.Commands;
using DimplowTools.Data;
using DimplowTools.Tools;
using GraphShape;
using GraphShape.Controls;
using GraphShape.Utils;
using QuikGraph;
using QuikGraph.Algorithms;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DimplowTools.Models
{
    internal class UndirectedGraphModel : ObservableClass
    {
        public int[][] CreateField(int size)
        {
            int index;
            int[][] field = new int[size][];
            for (index = 0; index < size; index++)
                field[index] = new int[size];
            return field;
        }

        public BidirectionalGraph<Vertex, SEdge<Vertex>> GenerateUndirectedVertices(int amount, int radius, int size = 10000)
        {
            BidirectionalGraph<Vertex, SEdge<Vertex>> graph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
            int index;
            Random random = new Random();
            int[][] field = CreateField(size);
            int x, y;
            for (index = 0; index < amount; index++)
            {
                do
                {
                    x = random.Next(field.Length);
                    y = random.Next(field.Length);
                }
                while (field[x][y] == 1);
                graph.AddVertex(new Vertex(radius, x, y, index, amount));
            }
            return graph;
        }

        public BidirectionalGraph<Vertex, SEdge<Vertex>> GenerateUndirectedEdges(BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            List<Vertex> vertices = graph.Vertices.ToList();
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
                        graph.AddEdge(new SEdge<Vertex>(vertices[index1], vertices[index2]));
                        vertices[index1].Neighbors[index2] = true;
                        vertices[index2].Neighbors[index1] = true;
                    }
                }
            }
            OnPropertyChanged();
            return graph;
        }

        public int Ki(int i)
        {
            return (int)Math.Pow(2, i);
        }

        public void ContractVertices(BidirectionalGraph<Vertex, SEdge<Vertex>> graph, Vertex rootVertex, Vertex contractVertex)
        {
            //ВОзможно, эта хуйнря все это время работала неправильно. Надо было стягивать вершины, и, как в алгоритме каргера, удалять только те которые остались между стягиваемыми,
            //а остальные оставлять как в мультиграфе.
            List<SEdge<Vertex>> inEdges = graph.InEdges(contractVertex).ToList();
            rootVertex.Neighbors[contractVertex.ID] = false;
            for (int j = 0; j < inEdges.Count; j++)
            {
                inEdges[j].Source.Neighbors[contractVertex.ID] = false;
                if (!rootVertex.Neighbors[inEdges[j].Source.ID] && rootVertex.ID != inEdges[j].Source.ID)
                {
                    graph.AddEdge(new SEdge<Vertex>(inEdges[j].Source, rootVertex));
                    graph.AddEdge(new SEdge<Vertex>(rootVertex, inEdges[j].Source));
                    rootVertex.Neighbors[inEdges[j].Source.ID] = true;
                    inEdges[j].Source.Neighbors[rootVertex.ID] = true;
                }

            }
            graph.RemoveVertex(contractVertex);
        }

        private BidirectionalGraph<Vertex, SEdge<Vertex>> FindRootCut(int Ki0, Vertex rootVertex, BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            BidirectionalGraph<Vertex, SEdge<Vertex>> subGraph = graph.Clone();
            List<Vertex> vertices = subGraph.Vertices.ToList();
            int i;
            for (i = 0; i < subGraph.VertexCount; i++)
                if (subGraph.InDegree(vertices[i]) >= 3 * Ki0 && rootVertex != vertices[i])
                    ContractVertices(subGraph, rootVertex, vertices[i]);
            graph = subGraph.Clone();
            OnPropertyChanged();
            return graph;
        }

        public BidirectionalGraph<Vertex, SEdge<Vertex>> FindSinkCuts(BidirectionalGraph<Vertex, SEdge<Vertex>> graph, int sinkVertex)
        {
            int l = (int)Math.Floor(Math.Sqrt(graph.VertexCount));
            int i0 = (int)Math.Truncate(Math.Log10(l));
            int Ki0 = Ki(i0);
            BidirectionalGraph<Vertex, SEdge<Vertex>> graph1 = FindRootCut(Ki0, graph.Vertices.ElementAt(sinkVertex), graph);
            return graph1;
        }

        private bool[] HelperDFS(Vertex vertex, bool[] visited, BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            visited[vertex.ID] = true;
            foreach (Vertex neighbor in graph.GetNeighbors(vertex))
            {
                if (!visited[neighbor.ID])
                    visited = HelperDFS(neighbor, visited, graph);
            }
            return visited;
        }

        public bool DFS(BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            bool[] visited = new bool[graph.VertexCount];
            visited = HelperDFS(graph.Vertices.First(), visited, graph);
            foreach (bool item in visited)
                if (!item)
                    return false;
            return true;
        }

        public void KargerContractVertices(BidirectionalGraph<Vertex, SEdge<Vertex>> graph, Vertex rootVertex, Vertex contractVertex, SEdge<Vertex> contractArc)
        {

            List<SEdge<Vertex>> inEdges = graph.InEdges(contractVertex).ToList();
            for (int j = 0; j < inEdges.Count; j++)
                if (inEdges[j].Source.ID != rootVertex.ID)
                {
                    graph.AddEdge(new SEdge<Vertex>(inEdges[j].Source, rootVertex));
                    graph.AddEdge(new SEdge<Vertex>(rootVertex, inEdges[j].Source));
                }

            graph.RemoveVertex(contractVertex);
        }

        public int KargerSteinAlgorithm(BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            BidirectionalGraph<Vertex, SEdge<Vertex>> subGraph = graph.Clone();
            Random random = new Random();
            while (subGraph.VertexCount > 2)
            {
                SEdge<Vertex> conctractingEdge = subGraph.Edges.ElementAt(random.Next(0, subGraph.EdgeCount));
                KargerContractVertices(subGraph, conctractingEdge.Source, conctractingEdge.Target, conctractingEdge);
            }
            return subGraph.EdgeCount;
        }

        private BidirectionalGraph<Vertex, SEdge<Vertex>> CreateSubgraph(BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            BidirectionalGraph<Vertex, SEdge<Vertex>> subgraph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
            foreach (Vertex vertex in graph.Vertices)
                subgraph.AddVertex(new Vertex(vertex.Radius, vertex.X, vertex.Y, vertex.ID, graph.VertexCount));
            foreach (SEdge<Vertex> edge in graph.Edges)
                subgraph.AddEdge(new SEdge<Vertex>(subgraph.Vertices.ElementAt(edge.Source.ID), subgraph.Vertices.ElementAt(edge.Target.ID)));

            return subgraph;

        }

        public int FindSingletoneCut(BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            List<Vertex> vertices = graph.Vertices.ToList();
            int minDegree = int.MaxValue;
            for (int i = 0; i < vertices.Count(); i++)
                if (graph.Degree(vertices[i]) / 2 < minDegree)
                    minDegree = graph.Degree(vertices[i]) / 2;
            return minDegree;
        }

        public Pair<int, int> FindConnectivityFunction(BidirectionalGraph<Vertex, SEdge<Vertex>> graph, int size, string filepath)
        {
            List<int> vertexMinCuts = new List<int>();
            List<int> withoutSinkMinCuts = new List<int>();
            int globalMinCut = int.MaxValue;
            int sinkMinCut = int.MaxValue;
            int singletoneCut = FindSingletoneCut(graph);
            for (int i = 0; i < graph.VertexCount; i++)
            {
                BidirectionalGraph<Vertex, SEdge<Vertex>> subGraph = CreateSubgraph(graph);
                withoutSinkMinCuts.Add(KargerSteinAlgorithm(subGraph) / 2);
                if (withoutSinkMinCuts.Last() < globalMinCut)
                    globalMinCut = withoutSinkMinCuts.Last();

                subGraph = CreateSubgraph(graph);
                int currentMinCut = KargerSteinAlgorithm(subGraph) / 2;
                if (currentMinCut < sinkMinCut)
                    sinkMinCut = currentMinCut;
                vertexMinCuts.Add(currentMinCut);
            }

            StreamWriter streamWriter = new StreamWriter(filepath, append: true);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("Time of start:" + DateTime.Now);
            streamWriter.WriteLine("Graph settings.");
            streamWriter.WriteLine("a. Amount of vertices:" + graph.VertexCount.ToString());
            streamWriter.WriteLine("b. Radius:" + graph.Vertices.First().Radius.ToString());
            streamWriter.WriteLine("c. Field size:" + size);
            streamWriter.WriteLine("d. Singletone cut:" + singletoneCut.ToString());

            streamWriter.WriteLine("e. Connectivity function:");
            for (int i = 0; i < vertexMinCuts.Count - 1; i++)
                streamWriter.Write(vertexMinCuts[i].ToString() + "-");
            streamWriter.WriteLine(vertexMinCuts.Last().ToString());

            streamWriter.WriteLine("f. Connectivity function without sink:");
            for (int i = 0; i < withoutSinkMinCuts.Count - 1; i++)
                streamWriter.Write(withoutSinkMinCuts[i].ToString() + "-");
            streamWriter.WriteLine(withoutSinkMinCuts.Last().ToString());

            streamWriter.WriteLine("g. Global min-cut:" + globalMinCut.ToString());

            streamWriter.WriteLine("h. Sinked min-cut:" + sinkMinCut.ToString());

            streamWriter.WriteLine("Time of end:" + DateTime.Now);

            streamWriter.Close();

            int deletedEdges = 0, deletedVertices = 0;
            int minCut = Math.Min(globalMinCut, sinkMinCut);
            if (globalMinCut == singletoneCut)
                return new Pair<int, int>(singletoneCut, singletoneCut);
            else
            {
                for (int i = 0; i < vertexMinCuts.Count; i++)
                {
                    if (vertexMinCuts[i] >= minCut)
                        vertexMinCuts[i] = 0;
                    else
                    {
                        vertexMinCuts[i] = minCut - vertexMinCuts[i];
                        deletedEdges += vertexMinCuts[i];
                        deletedVertices++;
                    }
                    if (deletedEdges >= minCut)
                        break;
                }
            }
            return new Pair<int, int>(deletedVertices + 1, minCut);
        }

        public Pair<int, int> FindConnectivityFunctionWithoutWriteFile(BidirectionalGraph<Vertex, SEdge<Vertex>> graph)
        {
            List<int> vertexMinCuts = new List<int>();
            List<int> withoutSinkMinCuts = new List<int>();
            int globalMinCut = int.MaxValue;
            int sinkMinCut = int.MaxValue;
            int singletoneCut = FindSingletoneCut(graph);
            for (int i = 0; i < graph.VertexCount; i++)
            {
                BidirectionalGraph<Vertex, SEdge<Vertex>> subGraph = CreateSubgraph(graph);
                withoutSinkMinCuts.Add(KargerSteinAlgorithm(subGraph) / 2);
                if (withoutSinkMinCuts.Last() < globalMinCut)
                    globalMinCut = withoutSinkMinCuts.Last();

                subGraph = CreateSubgraph(graph);
                int currentMinCut = KargerSteinAlgorithm(subGraph) / 2;
                if (currentMinCut < sinkMinCut)
                    sinkMinCut = currentMinCut;
                vertexMinCuts.Add(currentMinCut);
            }

            int deletedEdges = 0, deletedVertices = 0;
            int minCut = Math.Min(globalMinCut, sinkMinCut);
            if (globalMinCut == singletoneCut)
                return new Pair<int, int>(singletoneCut, singletoneCut);
            else
            {
                for (int i = 0; i < vertexMinCuts.Count; i++)
                {
                    if (vertexMinCuts[i] >= minCut)
                        vertexMinCuts[i] = 0;
                    else
                    {
                        vertexMinCuts[i] = minCut - vertexMinCuts[i];
                        deletedEdges += vertexMinCuts[i];
                        deletedVertices++;
                    }
                    if (deletedEdges >= minCut)
                        break;
                }
            }
            return new Pair<int, int>(deletedVertices + 1, minCut);
        }

        public void ResearchHelper(object undirectedGraphData)
        {
            UndirectedGraphData udg = undirectedGraphData as UndirectedGraphData;
            BidirectionalGraph<Vertex, SEdge<Vertex>> graph;
            do
            {
                graph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
                graph = GenerateUndirectedVertices(udg.AmountOfVertices, udg.Radius, udg.FieldSize);
                graph = GenerateUndirectedEdges(graph);
            } while (!DFS(graph));
            Pair<int, int> pair = FindConnectivityFunctionWithoutWriteFile(graph);
            results[udg.ID] = pair;
        }

        List<Pair<int, int>> results = new List<Pair<int, int>>();

        public void ConnectivityResearch(int amountOfVertices, int radius, int fieldSize, int graphAmount, string filepath)
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < graphAmount; i++)
            {
                results.Add(new Pair<int, int>(0, 0));
                UndirectedGraphData ugd = new UndirectedGraphData(amountOfVertices, radius, fieldSize, graphAmount, i);
                Task task = new Task(() =>
                {
                    ResearchHelper(ugd);
                });
                //task.Start();
                tasks.Add(task);
            }
            double averageCut = 0, averageVerticesDeleted = 0;
            StreamWriter streamWriter = new StreamWriter(filepath, append: true);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("Time of start:" + DateTime.Now);
            streamWriter.WriteLine("Graph settings:");
            streamWriter.WriteLine("a. Amount of vertices:" + amountOfVertices);
            streamWriter.WriteLine("b. Radius:" + radius);
            streamWriter.WriteLine("c. Field size:" + fieldSize);
            streamWriter.WriteLine("d. AmountOfGraphs:" + graphAmount);
            streamWriter.WriteLine("e. All cuts in format vertex/edges:");
            for (int i = 0; i < tasks.Count; i++)
            {
                if (i % 5 == 0)
                {
                    tasks[i].Start();
                    tasks[i + 1].Start();
                    tasks[i + 2].Start();
                    tasks[i + 3].Start();
                    tasks[i + 4].Start();
                }
                while (tasks[i].Status != TaskStatus.RanToCompletion)
                    tasks[i].Wait();
                averageVerticesDeleted += results[i].First;
                averageCut += results[i].Second;
                if (i != tasks.Count - 1)
                    streamWriter.Write(results[i].First + "/" + results[i].Second + "-");
            }
            streamWriter.WriteLine(results.Last().First + "/" + results.Last().Second);
            averageVerticesDeleted /= graphAmount;
            averageCut /= graphAmount;
            streamWriter.WriteLine("f. Average vertex deletion:" + averageVerticesDeleted);
            streamWriter.WriteLine("g. Average edges deletion:" + averageCut);
            streamWriter.WriteLine("Time of end:" + DateTime.Now);
            streamWriter.Close();
        }

        private void MoveResearchPointsHelper(object MVPData)
        {
            MoveResearchPointsData mvpData = MVPData as MoveResearchPointsData;
            BidirectionalGraph<Vertex, SEdge<Vertex>> subgraph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
            subgraph = GenerateUndirectedVertices(mvpData.VertexAmount, mvpData.Radius, mvpData.FieldSize);
            subgraph = GenerateUndirectedEdges(subgraph);
            int averageCut = 0;
            for (int i = 0; i < mvpData.MoveAmount; i++)
            {
                if (DFS(subgraph))
                {
                    int mincut = int.MaxValue;
                    for (int j = 0; j < subgraph.VertexCount / 2; j++)
                    {
                        int currentMincut = KargerSteinAlgorithm(subgraph);
                        if (mincut > currentMincut)
                            mincut = currentMincut;
                    }
                    averageCut += mincut;
                }
                subgraph = MoveVertices(subgraph, mvpData.FieldSize, mvpData.MinRadiusMovement, mvpData.MaxRadiusMovement);
            }
            moveCutResults[mvpData.ID] = averageCut / mvpData.MoveAmount;
        }

        List<double> moveCutResults;
        public void MoveResearchPoints(int vertexAmount, int radius, int graphAmount, int moveAmount, int minRadiusMovement, int maxRadiusMovement, int fieldSize, string filepath)
        {
            moveCutResults = new List<double>(graphAmount);
            StreamWriter streamWriter = new StreamWriter(filepath, append: true);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("Time of start:" + DateTime.Now);
            streamWriter.WriteLine("Graph settings:");
            streamWriter.WriteLine("a. Amount of vertices:" + vertexAmount);
            streamWriter.WriteLine("b. Radius of vertices:" + radius);
            streamWriter.WriteLine("b.1 Minimum radius of movement:" + minRadiusMovement);
            streamWriter.WriteLine("b.2 Maximum radius of movement:" + maxRadiusMovement);
            streamWriter.WriteLine("c. Field size:" + fieldSize);
            streamWriter.WriteLine("d. Amount of movements:" + moveAmount);
            streamWriter.WriteLine("e. Amount of graphs:" + graphAmount);
            streamWriter.WriteLine("f. All edge cuts:");
            double averageEdgeCut = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < graphAmount; i++)
            {
                MoveResearchPointsData mvpData = new MoveResearchPointsData(vertexAmount, radius, moveAmount, minRadiusMovement, maxRadiusMovement, fieldSize, i);
                Task task = new Task(() =>
                {
                    MoveResearchPointsHelper(mvpData);
                });
                tasks.Add(task);
                moveCutResults.Add(-1);
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                if (i % 5 == 0)
                {
                    tasks[i].Start();
                    tasks[i + 1].Start();
                    tasks[i + 2].Start();
                    tasks[i + 3].Start();
                    tasks[i + 4].Start();
                }
                while (tasks[i].Status != TaskStatus.RanToCompletion)
                    tasks[i].Wait();

                streamWriter.Write(moveCutResults[i] + "-");
                averageEdgeCut += moveCutResults[i];
            }

            streamWriter.WriteLine("/");
            streamWriter.WriteLine("f. Average edge cut:" + averageEdgeCut / graphAmount);
            streamWriter.WriteLine("Time of end:" + DateTime.Now);
            streamWriter.Close();
        }

        public BidirectionalGraph<Vertex, SEdge<Vertex>> MoveVertices(BidirectionalGraph<Vertex, SEdge<Vertex>> graph, int fieldSize, int minRadiusMovement, int maxRadiusMovement)
        {
            Random random = new Random();
            int[][] field = CreateField(fieldSize);
            int x, y, stepX, stepY;

            while (graph.Edges.Count() > 0)
            {
                graph.Edges.First().Target.Neighbors[graph.Edges.First().Source.ID] = false;
                graph.Edges.First().Source.Neighbors[graph.Edges.First().Target.ID] = false;
                graph.RemoveEdge(graph.Edges.First());
            }

            //Заранее на поле пометим, в какую точку должна придти вершина. ПРосто чтобы знатоь, что два одинаковых узла не двигаются в одну точку.
            foreach (Vertex vertex in graph.Vertices)
            {
                if (
                        (vertex.MoveGlobalX == -1 && vertex.MoveGlobalY == -1) ||
                        (vertex.MoveGlobalX == vertex.X && vertex.MoveGlobalY == vertex.Y)
                   )
                {
                    do
                    {
                        x = random.Next(0, fieldSize - 1);
                        y = random.Next(0, fieldSize - 1);
                    }
                    while (field[x][y] == 2);
                    vertex.MoveGlobalX = x;
                    vertex.MoveGlobalY = y;
                    field[x][y] = 2;
                }
                else
                {
                    field[vertex.MoveGlobalX][vertex.MoveGlobalY] = 2;
                }

                if (vertex.StepSize == -1)
                    vertex.StepSize = random.Next(minRadiusMovement, maxRadiusMovement);
            }

            //Разметили поле и почистили старые ненужные вершины, теперь подвигаем их в нужное направление в рамках заданного радиуса
            for (int index = 0; index < graph.VertexCount; index++)
            {
                stepX = graph.Vertices.ElementAt(index).MoveGlobalX - graph.Vertices.ElementAt(index).X;
                if (stepX > graph.Vertices.ElementAt(index).StepSize)
                    stepX = graph.Vertices.ElementAt(index).StepSize;
                if (stepX < -graph.Vertices.ElementAt(index).StepSize)
                    stepX = -graph.Vertices.ElementAt(index).StepSize;

                stepY = graph.Vertices.ElementAt(index).MoveGlobalY - graph.Vertices.ElementAt(index).Y;
                if (stepY > graph.Vertices.ElementAt(index).StepSize)
                    stepY = graph.Vertices.ElementAt(index).StepSize;
                if (stepY < -graph.Vertices.ElementAt(index).StepSize)
                    stepY = -graph.Vertices.ElementAt(index).StepSize;

                x = graph.Vertices.ElementAt(index).X + stepX;
                y = graph.Vertices.ElementAt(index).Y + stepY;
                field[graph.Vertices.ElementAt(index).X][graph.Vertices.ElementAt(index).Y] = 0;
                field[x][y] = 1;
                graph.Vertices.ElementAt(index).X = x;
                graph.Vertices.ElementAt(index).Y = y;
            }
            graph = GenerateUndirectedEdges(graph);
            return graph;
        }

        private void MoveResearchDirectionsHelper(object MVPData)
        {
            MoveResearchPointsData mvpData = MVPData as MoveResearchPointsData;
            BidirectionalGraph<Vertex, SEdge<Vertex>> subgraph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
            subgraph = GenerateUndirectedVertices(mvpData.VertexAmount, mvpData.Radius, mvpData.FieldSize);
            subgraph = GenerateUndirectedEdges(subgraph);
            int averageCut = 0;
            for (int i = 0; i < mvpData.MoveAmount; i++)
            {
                if (DFS(subgraph))
                {
                    int mincut = int.MaxValue;
                    for (int j = 0; j < subgraph.VertexCount / 2; j++)
                    {
                        int currentMincut = KargerSteinAlgorithm(subgraph);
                        if (mincut > currentMincut)
                            mincut = currentMincut;
                    }
                    averageCut += mincut;
                }
                subgraph = MoveVerticesDirections(subgraph, mvpData.FieldSize, mvpData.MinRadiusMovement, mvpData.MaxRadiusMovement);
            }
            moveCutResults[mvpData.ID] = averageCut / mvpData.MoveAmount;
        }


        public void MoveResearchDirections(int vertexAmount, int radius, int graphAmount, int moveAmount, int minRadiusMovement, int maxRadiusMovement, int fieldSize, string filepath)
        {
            moveCutResults = new List<double>(graphAmount);
            StreamWriter streamWriter = new StreamWriter(filepath, append: true);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("Time of start:" + DateTime.Now);
            streamWriter.WriteLine("Graph settings:");
            streamWriter.WriteLine("a. Amount of vertices:" + vertexAmount);
            streamWriter.WriteLine("b. Radius of vertices:" + radius);
            streamWriter.WriteLine("b.1 Minimum radius of movement:" + minRadiusMovement);
            streamWriter.WriteLine("b.2 Maximum radius of movement:" + maxRadiusMovement);
            streamWriter.WriteLine("c. Field size:" + fieldSize);
            streamWriter.WriteLine("d. Amount of movements:" + moveAmount);
            streamWriter.WriteLine("e. Amount of graphs:" + graphAmount);
            streamWriter.WriteLine("f. All edge cuts:");
            double averageEdgeCut = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < graphAmount; i++)
            {
                MoveResearchPointsData mvpData = new MoveResearchPointsData(vertexAmount, radius, moveAmount, minRadiusMovement, maxRadiusMovement, fieldSize, i);
                Task task = new Task(() =>
                {
                    MoveResearchDirectionsHelper(mvpData);
                });
                tasks.Add(task);
                moveCutResults.Add(-1);
            }
            
            for (int i = 0; i < tasks.Count; i++)
            {
                if (i % 5 == 0)
                {
                    tasks[i].Start();
                    tasks[i + 1].Start();
                    tasks[i + 2].Start();
                    tasks[i + 3].Start();
                    tasks[i + 4].Start();
                }
                while (tasks[i].Status != TaskStatus.RanToCompletion)
                    tasks[i].Wait();

                streamWriter.Write(moveCutResults[i] + "-");
                averageEdgeCut += moveCutResults[i];
            }

            streamWriter.WriteLine("/");
            streamWriter.WriteLine("f. Average edge cut:" + averageEdgeCut / graphAmount);
            streamWriter.WriteLine("Time of end:" + DateTime.Now);
            streamWriter.Close();
        }

        public BidirectionalGraph<Vertex, SEdge<Vertex>> MoveVerticesDirections(BidirectionalGraph<Vertex, SEdge<Vertex>> graph, int fieldSize, int minRadiusMovement, int maxRadiusMovement)
        {
            Random random = new Random();
            int[][] field = CreateField(fieldSize);
            int x, y;

            while (graph.Edges.Count() > 0)
            {
                graph.Edges.First().Target.Neighbors[graph.Edges.First().Source.ID] = false;
                graph.Edges.First().Source.Neighbors[graph.Edges.First().Target.ID] = false;
                graph.RemoveEdge(graph.Edges.First());
            }

            foreach (Vertex vertex in graph.Vertices)
            {
                if (
                        (vertex.MoveGlobalX == -1 && vertex.MoveGlobalY == -1) ||
                        vertex.X == field.Length - 1 || vertex.X == 0 ||
                        vertex.Y == field.Length - 1 || vertex.Y == 0
                   )
                {
                    x = random.Next(minRadiusMovement, maxRadiusMovement);
                    y = random.Next(minRadiusMovement, maxRadiusMovement);

                    if (vertex.X == field.Length - 1)
                        x = -x;
                    if (vertex.Y == field.Length - 1)
                        y = -y;

                    vertex.MoveGlobalX = x;
                    vertex.MoveGlobalY = y;
                }
            }

            //Разметили поле и почистили старые ненужные вершины, теперь подвигаем их в нужное направление в рамках заданного радиуса
            for (int index = 0; index < graph.VertexCount; index++)
            {
                x = graph.Vertices.ElementAt(index).X + graph.Vertices.ElementAt(index).MoveGlobalX;
                y = graph.Vertices.ElementAt(index).Y + graph.Vertices.ElementAt(index).MoveGlobalY;

                if (y >= field.Length)
                    y = field.Length - 1;
                if (y <= -1)
                    y = 0;

                if (x >= field.Length)
                    x = field.Length - 1;
                if (x <= -1)
                    x = 0;

                field[graph.Vertices.ElementAt(index).X][graph.Vertices.ElementAt(index).Y] = 0;
                field[x][y] = 1;
                graph.Vertices.ElementAt(index).X = x;
                graph.Vertices.ElementAt(index).Y = y;
            }
            graph = GenerateUndirectedEdges(graph);
            return graph;
        }
    }
}
