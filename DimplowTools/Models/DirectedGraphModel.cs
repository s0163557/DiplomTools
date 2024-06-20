using DimplowTools.Commands;
using DimplowTools.Data;
using DimplowTools.Tools;
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
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace DimplowTools.Models
{
    internal class DirectedGraphModel : ObservableClass
    {
        BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> _graph;
        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }
        public int[][] CreateField(int size)
        {
            int index;
            int[][] field = new int[size][];
            for (index = 0; index < size; index++)
                field[index] = new int[size];
            return field;
        }

        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> GenerateDirectedVertices(int amountOfVertices, int minRadius, int maxRadius, int[][] fieldSize,
                                                                                            int minWeight, int maxWeight)
        {
            int index;
            Random random = new Random();
            int radius, x, y;
            BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph = new BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>>();
            for (index = 0; index < amountOfVertices; index++)
            {
                radius = random.Next(minRadius, maxRadius);
                do
                {
                    x = random.Next(fieldSize.Length);
                    y = random.Next(fieldSize.Length);
                }
                while (fieldSize[x][y] == 1);
                graph.AddVertex(new WeightedVertex(radius, x, y, index, random.Next(minWeight, maxWeight)));
            }
            return graph;
        }

        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> GenerateDirectedEdges(BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph)
        {
            List<WeightedVertex> vertices = graph.Vertices.ToList();
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
                        graph.AddEdge(new DirectedEdge<WeightedVertex>(vertices[index1], vertices[index2], vertices[index1].SignalPower));
                }
            }
            OnPropertyChanged();
            return graph;
        }

        public int Ki(int i)
        {
            return (int)Math.Pow(2, i);
        }

        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> FindRootCut(int Ki0, WeightedVertex rootWeightedVertex, BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph)
        {
            BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> subGraph = graph.Clone();
            List<WeightedVertex> vertices = graph.Vertices.ToList();
            int i, j;

            for (i = 1; i < subGraph.VertexCount; i++)
            {
                if (subGraph.InDegree(vertices[i]) >= 2 * Ki0)
                {
                    List<DirectedEdge<WeightedVertex>> inEdges = subGraph.InEdges(vertices[i]).ToList();
                    for (j = 0; j < inEdges.Count; j++)
                        if (rootWeightedVertex != inEdges[j].Source)
                        {
                            subGraph.TryGetInEdges(rootWeightedVertex, out IEnumerable<DirectedEdge<WeightedVertex>> list);
                            if (list.Where(arc => arc.Source == inEdges[j].Source).Count() == 0)
                                subGraph.AddEdge(new DirectedEdge<WeightedVertex>(inEdges[j].Source, rootWeightedVertex, inEdges[j].Weight));
                            else
                                subGraph.InEdges(rootWeightedVertex).Where(arc => arc.Source == inEdges[j].Source).First().Weight += inEdges[j].Weight;
                        }

                    List<DirectedEdge<WeightedVertex>> outEdges = subGraph.OutEdges(vertices[i]).ToList();
                    for (j = 0; j < outEdges.Count; j++)
                        if (rootWeightedVertex != outEdges[j].Target)
                        {
                            if (!subGraph.GetOutNeighbors(rootWeightedVertex).Contains(outEdges[j].Target))
                                subGraph.AddEdge(new DirectedEdge<WeightedVertex>(rootWeightedVertex, outEdges[j].Target, outEdges[j].Weight));
                            else
                                subGraph.OutEdges(rootWeightedVertex).Where(arc => arc.Target == outEdges[j].Target).First().Weight += outEdges[j].Weight;
                        }

                    subGraph.RemoveVertex(vertices[i]);
                }
            }
            i = 0;
            foreach (WeightedVertex WeightedVertex in subGraph.Vertices)
                WeightedVertex.ID = i++;

            graph = subGraph.Clone();
            OnPropertyChanged();
            return graph;
        }

        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> FindSinkCuts(BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph)
        {
            int l = (int)Math.Truncate(Math.Sqrt(graph.VertexCount));
            int i0 = (int)Math.Log10(l);
            int Ki0 = Ki(i0);
            //Переберём все вершины, сразу авансом для функции связности
            List<WeightedVertex> vertices = graph.Vertices.ToList();
            return FindRootCut(Ki0, vertices[0], graph);
        }

        private int _time = 0;
        public void UtilSCC(WeightedVertex u, WeightedVertex prev, List<int> low, List<int> disc, List<bool> stackMember, Stack<Pair<WeightedVertex, WeightedVertex>> st,
            List<List<Pair<WeightedVertex, WeightedVertex>>> stronglyConnectedComponents, BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph)
        {
            disc[u.ID] = low[u.ID] = _time;
            _time++;
            stackMember[u.ID] = true;
            st.Push(new Pair<WeightedVertex, WeightedVertex>(u, prev));

            foreach (WeightedVertex neighbor in graph.GetOutNeighbors(graph.Vertices.ElementAt(u.ID)))
            {
                if (disc[neighbor.ID] == -1)
                {
                    UtilSCC(neighbor, u, low, disc, stackMember, st, stronglyConnectedComponents, graph);
                    low[u.ID] = Math.Min(low[u.ID], low[neighbor.ID]);
                }
                else if (stackMember[neighbor.ID] == true)
                    low[u.ID] = Math.Min(low[u.ID], disc[neighbor.ID]);
            }

            int k = -1;
            if (low[u.ID] == disc[u.ID])
            {
                List<Pair<WeightedVertex, WeightedVertex>> component = new List<Pair<WeightedVertex, WeightedVertex>>();
                while (k != u.ID)
                {
                    component.Add(st.Pop());
                    k = component.Last().First.ID;
                    stackMember[k] = false;
                }
                stronglyConnectedComponents.Add(component);
            }
        }

        public List<List<Pair<WeightedVertex, WeightedVertex>>> TrajanSCC(BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph)
        {
            //Будем хранить не только посещенную вершину, но и её предшественника. Если предшественник совпадает с текущей, то эта вершина - начальная.
            List<List<Pair<WeightedVertex, WeightedVertex>>> stronglyConnectedComponents = new List<List<Pair<WeightedVertex, WeightedVertex>>>();
            List<int> disc = new List<int>();
            List<int> low = new List<int>();
            List<bool> stackMember = new List<bool>();
            disc.Capacity = low.Capacity = stackMember.Capacity = graph.VertexCount;
            for (int i = 0; i < graph.VertexCount; i++)
            {
                disc.Add(-1);
                low.Add(-1);
                stackMember.Add(false);
            }
            Stack<Pair<WeightedVertex, WeightedVertex>> st = new Stack<Pair<WeightedVertex, WeightedVertex>>();
            for (int i = 0; i < graph.VertexCount; i++)
            {
                if (disc[i] == -1)
                    UtilSCC(graph.Vertices.ElementAt(i), graph.Vertices.ElementAt(i), low, disc, stackMember, st, stronglyConnectedComponents, graph);
            }

            return stronglyConnectedComponents;
        }

        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graphFromSCC(List<List<Pair<WeightedVertex, WeightedVertex>>> stronglyConnectedComponents)
        {
            BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> treeGraph = new BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>>();
            for (int i = 0; i < stronglyConnectedComponents.Count; i++)
            {
                treeGraph.AddVertex(stronglyConnectedComponents[i][0].First);
                treeGraph.AddVertex(stronglyConnectedComponents[i][0].Second);
                treeGraph.AddEdge(new DirectedEdge<WeightedVertex>(stronglyConnectedComponents[i][0].Second, stronglyConnectedComponents[i][0].First, 0));
                for (int j = 1; j < stronglyConnectedComponents[i].Count; j++)
                {
                    if (stronglyConnectedComponents[i][j].Second != stronglyConnectedComponents[i][j].First)
                    {
                        if (!treeGraph.ContainsVertex(stronglyConnectedComponents[i][j].First))
                            treeGraph.AddVertex(stronglyConnectedComponents[i][j].First);
                        if (!treeGraph.ContainsVertex(stronglyConnectedComponents[i][j].Second))
                            treeGraph.AddVertex(stronglyConnectedComponents[i][j].Second);
                        treeGraph.AddEdge(new DirectedEdge<WeightedVertex>(stronglyConnectedComponents[i][j].Second, stronglyConnectedComponents[i][j].First, 0));
                    }
                }
            }
            return treeGraph;
        }

        private List<ResearchResult> _results = new List<ResearchResult>();

        private void GraphResearch(object graphData)
        {
            Random random = new Random();
            GraphData gd = graphData as GraphData;
            int[][] field = CreateField(gd.FieldSize);
            BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph = GenerateDirectedVertices(gd.AmountOfVertices, gd.MinRadius, gd.MaxRadius, field,
                                                                                                gd.MinWeight, gd.MaxWeight);
            graph = GenerateDirectedEdges(graph);

            _results[gd.TaskId].MinCut = int.MaxValue;
            _results[gd.TaskId].MaxFlow = int.MaxValue;
            for (int i = 0; i <= Math.Sqrt(gd.AmountOfVertices) * 2; i++)
            {
                BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> subgraph = graph.Clone();
                WeightedOHAlgorithm weightedOHAlgorithm = new WeightedOHAlgorithm(subgraph, random.Next(0, graph.VertexCount));
                _results[gd.TaskId].MaxFlow = Math.Min(weightedOHAlgorithm.FindMinCut(), _results[gd.TaskId].MaxFlow);
                _results[gd.TaskId].MinCut = Math.Min(weightedOHAlgorithm.MinCutAmount, _results[gd.TaskId].MinCut);
            }

        }

        public void MultiThreadGraphResearch(int amountOfVertices, int minRadius, int maxRadius, int minWeight, int maxWeight, int amountOfGraphs, string filepath, int size = 10000)
        {
                CreateField(size);
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < amountOfGraphs; i++)
                {
                    GraphData gd = new GraphData(amountOfVertices, minRadius, maxRadius, minWeight, maxWeight, i, size);
                    Task task = new Task(() => GraphResearch(gd));
                    tasks.Add(task);
                    if (_results.Count <= amountOfGraphs)
                        _results.Add(new ResearchResult());
                    _results[i].MinCut = int.MaxValue;
                    _results[i].MaxFlow = int.MaxValue;
                }

                StreamWriter streamWriter = new StreamWriter(filepath, append: true);
                streamWriter.WriteLine("Time of start:" + DateTime.Now);
                streamWriter.WriteLine("Graph settings.");
                streamWriter.WriteLine("a. Amount of vertices:" + amountOfVertices.ToString());
                streamWriter.WriteLine("b. Minimum radius:" + minRadius.ToString());
                streamWriter.WriteLine("c. Maximum radius:" + maxRadius.ToString());
                streamWriter.WriteLine("d. Minimum weight:" + minWeight.ToString());
                streamWriter.WriteLine("e. Maximum weight:" + maxWeight.ToString());
                streamWriter.WriteLine("f. Amount of graphs:" + amountOfGraphs.ToString());
                streamWriter.WriteLine("g. Field size:" + size.ToString());
                streamWriter.WriteLine("MaxFlow/MinCut");
                double averageMaxFlow = 0, averageMinCut = 0;
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

                    streamWriter.Write(_results[i].MaxFlow.ToString() + "/" + _results[i].MinCut.ToString() + "-");
                    averageMaxFlow += _results[i].MaxFlow;
                    averageMinCut += _results[i].MinCut;
                }

                streamWriter.WriteLine("///" + DateTime.Now);
                streamWriter.WriteLine("Time of end:" + DateTime.Now);
                streamWriter.WriteLine("Average max flow:" + averageMaxFlow / amountOfGraphs);
                streamWriter.WriteLine("Average min cut:" + averageMinCut / amountOfGraphs);

                streamWriter.Close();
        }

        //Модели движения
        private void MoveResearchPointsHelper(object MVPData)
        {
            Random random = new Random();
            DirectedMoveResearchPointsData mvpData = MVPData as DirectedMoveResearchPointsData;
            BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph = new BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>>();
            graph = GenerateDirectedVertices(mvpData.VertexAmount, mvpData.MinRadius, mvpData.MaxRadius, CreateField(mvpData.FieldSize), mvpData.MinWeight, mvpData.MaxWeight);
            graph = GenerateDirectedEdges(graph);
            int averageCut = 0;
            int averageMaxFlow = 0;
            for (int i = 0; i < mvpData.MoveAmount; i++)
            {
                if (TrajanSCC(graph).Count <= 2)
                {
                    int minCut = int.MaxValue;
                    int maxFlow = int.MaxValue;
                    for (int c = 0; c <= Math.Sqrt(graph.VertexCount) * 2; c++)
                    {
                        BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> subgraph = graph.Clone();
                        WeightedOHAlgorithm weightedOHAlgorithm = new WeightedOHAlgorithm(subgraph, random.Next(0, graph.VertexCount));
                        maxFlow = Math.Min(weightedOHAlgorithm.FindMinCut(), maxFlow);
                        minCut = Math.Min(weightedOHAlgorithm.MinCutAmount, minCut);
                    }
                    averageCut += minCut;
                    averageMaxFlow += maxFlow;
                }
                graph = MoveVertices(graph, mvpData.FieldSize, mvpData.MinRadiusMovement, mvpData.MaxRadiusMovement);
            }
            movePointsResults[mvpData.ID].MinCut = averageCut / mvpData.MoveAmount;
            movePointsResults[mvpData.ID].MaxFlow = averageMaxFlow / mvpData.MoveAmount;
        }

        List<ResearchResult> movePointsResults;
        public void MoveResearchPoints(int vertexAmount, int minRadius, int maxRadius, int minWeight, int maxWeight, int graphAmount, int moveAmount,
                                        int minRadiusMovement, int maxRadiusMovement, int fieldSize, string filepath)
        {
            movePointsResults = new List<ResearchResult>(graphAmount);
            StreamWriter streamWriter = new StreamWriter(filepath, append: true);
            streamWriter.WriteLine("");
            streamWriter.WriteLine("Time of start:" + DateTime.Now);
            streamWriter.WriteLine("Graph settings:");
            streamWriter.WriteLine("a. Amount of vertices:" + vertexAmount);
            streamWriter.WriteLine("b.1 Minimum radius of vertices:" + minRadius);
            streamWriter.WriteLine("b.2 Maximum radius of vertices:" + maxRadius);
            streamWriter.WriteLine("c.1 Minimum radius of movement:" + minRadiusMovement);
            streamWriter.WriteLine("c.2 Maximum radius of movement:" + maxRadiusMovement);
            streamWriter.WriteLine("d. Field size:" + fieldSize);
            streamWriter.WriteLine("e. Amount of movements:" + moveAmount);
            streamWriter.WriteLine("f. Amount of graphs:" + graphAmount);
            streamWriter.WriteLine("g. All edge cuts in format mincut/maxflow:");
            double averageEdgeCut = 0;
            double averageMaxFlow = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < graphAmount; i++)
            {
                DirectedMoveResearchPointsData mvpData = new DirectedMoveResearchPointsData(vertexAmount, minRadius, maxRadius, minWeight,
                                                                                            maxWeight, moveAmount, minRadiusMovement, maxRadiusMovement, fieldSize, i);
                Task task = new Task(() =>
                {
                    MoveResearchPointsHelper(mvpData);
                });
                tasks.Add(task);
                movePointsResults.Add(new ResearchResult(0, 0));
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

                streamWriter.Write(movePointsResults[i].MinCut + "/" + movePointsResults[i].MaxFlow + "-");
                averageEdgeCut += movePointsResults[i].MinCut;
                averageMaxFlow += movePointsResults[i].MaxFlow;
            }

            streamWriter.WriteLine("///");
            streamWriter.WriteLine("h. Average edge cut:" + averageEdgeCut / graphAmount);
            streamWriter.WriteLine("i. Average maximum flow:" + averageMaxFlow / graphAmount);
            streamWriter.WriteLine("Time of end:" + DateTime.Now);
            streamWriter.Close();
        }

        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> MoveVertices(BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph, int fieldSize, int minRadiusMovement, int maxRadiusMovement)
        {
            Random random = new Random();
            int[][] field = CreateField(fieldSize);
            int x, y, stepX, stepY;

            while (graph.Edges.Count() > 0)
                graph.RemoveEdge(graph.Edges.First());

            //Заранее на поле пометим, в какую точку должна придти вершина. ПРосто чтобы знатоь, что два одинаковых узла не двигаются в одну точку.
            foreach (WeightedVertex vertex in graph.Vertices)
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
            graph = GenerateDirectedEdges(graph);
            return graph;
        }

        private void MoveResearchDirectionsHelper(object MVPData)
        {
            Random random = new Random();
            DirectedMoveResearchPointsData mvpData = MVPData as DirectedMoveResearchPointsData;
            BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph = new BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>>();
            graph = GenerateDirectedVertices(mvpData.VertexAmount, mvpData.MinRadius, mvpData.MaxRadius, CreateField(mvpData.FieldSize), mvpData.MinWeight, mvpData.MaxWeight);
            graph = GenerateDirectedEdges(graph);
            int averageCut = 0;
            int averageMaxFlow = 0;
            for (int i = 0; i < mvpData.MoveAmount; i++)
            {
                if (TrajanSCC(graph).Count <= 2)
                {
                    int minCut = int.MaxValue;
                    int maxFlow = int.MaxValue;
                    for (int c = 0; c <= Math.Sqrt(graph.VertexCount) * 2; c++)
                    {
                        BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> subgraph = graph.Clone();
                        WeightedOHAlgorithm weightedOHAlgorithm = new WeightedOHAlgorithm(subgraph, random.Next(0, graph.VertexCount));
                        maxFlow = Math.Min(weightedOHAlgorithm.FindMinCut(), maxFlow);
                        minCut = Math.Min(weightedOHAlgorithm.MinCutAmount, minCut);
                    }
                    averageCut += minCut;
                    averageMaxFlow += maxFlow;
                }
                graph = MoveVerticesDirections(graph, mvpData.FieldSize, mvpData.MinRadiusMovement, mvpData.MaxRadiusMovement);
            }
            movePointsResults[mvpData.ID].MinCut = averageCut / mvpData.MoveAmount;
            movePointsResults[mvpData.ID].MaxFlow = averageMaxFlow / mvpData.MoveAmount;
        }


        public void MoveResearchDirections(int vertexAmount, int graphAmount, int moveAmount, int minRadiusMovement, int maxRadiusMovement, int fieldSize, int minRadius,
                                            int maxRadius, int minWeight, int maxWeight, string filepath)
        {
                movePointsResults = new List<ResearchResult>(graphAmount);
                StreamWriter streamWriter = new StreamWriter(filepath, append: true);
                streamWriter.WriteLine("");
                streamWriter.WriteLine("Time of start:" + DateTime.Now);
                streamWriter.WriteLine("Graph settings:");
                streamWriter.WriteLine("a. Amount of vertices:" + vertexAmount);
                streamWriter.WriteLine("b.1 Minimum radius of vertices:" + minRadius);
                streamWriter.WriteLine("b.2 Maximum radius of vertices:" + maxRadius);
                streamWriter.WriteLine("c.1 Minimum radius of movement:" + minRadiusMovement);
                streamWriter.WriteLine("c.2 Maximum radius of movement:" + maxRadiusMovement);
                streamWriter.WriteLine("d. Field size:" + fieldSize);
                streamWriter.WriteLine("e. Amount of movements:" + moveAmount);
                streamWriter.WriteLine("f. Amount of graphs:" + graphAmount);
                streamWriter.WriteLine("h. All edge cuts:");
                double averageEdgeCut = 0;
                double averageMaxFlow = 0;
                List<Task> tasks = new List<Task>();
                for (int i = 0; i < graphAmount; i++)
                {
                    DirectedMoveResearchPointsData mvpData = new DirectedMoveResearchPointsData(vertexAmount, minRadius, maxRadius, minWeight,
                                                                                                maxWeight, moveAmount, minRadiusMovement, maxRadiusMovement, fieldSize, i);
                    Task task = new Task(() =>
                    {
                        MoveResearchDirectionsHelper(mvpData);
                    });
                    tasks.Add(task);
                    movePointsResults.Add(new ResearchResult(0, 0));
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

                    streamWriter.Write(movePointsResults[i].MinCut + "/" + movePointsResults[i].MaxFlow + "-");
                    averageEdgeCut += movePointsResults[i].MinCut;
                    averageMaxFlow += movePointsResults[i].MaxFlow;
                }

                streamWriter.WriteLine("/");
                streamWriter.WriteLine("i. Average edge cut:" + averageEdgeCut / graphAmount);
                streamWriter.WriteLine("i. Average max flow:" + averageMaxFlow / graphAmount);
                streamWriter.WriteLine("Time of end:" + DateTime.Now);
                streamWriter.Close();
        }

        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> MoveVerticesDirections(BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph, int fieldSize, int minRadiusMovement, int maxRadiusMovement)
        {
            Random random = new Random();
            int[][] field = CreateField(fieldSize);
            int x, y;

            while (graph.Edges.Count() > 0)
                graph.RemoveEdge(graph.Edges.First());

            foreach (WeightedVertex vertex in graph.Vertices)
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
            graph = GenerateDirectedEdges(graph);
            return graph;
        }

    }
}
