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
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Models
{
    internal class DirectedGraphModel : ObservableClass
    {

        BidirectionalGraph<Vertex, DirectedEdge<Vertex>> _graph;
        public BidirectionalGraph<Vertex, DirectedEdge<Vertex>> Graph
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

        public void GenerateDirectedVertices(int amount, int minRadius, int maxRadius, int size = 10000)
        {
            int index;
            Random random = new Random();
            CreateField(size);
            int radius, x, y;
            _graph = new BidirectionalGraph<Vertex, DirectedEdge<Vertex>>();
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

        public void GenerateDirectedEdges()
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
                        _graph.AddEdge(new DirectedEdge<Vertex>(vertices[index1], vertices[index2]));
                }
            }
            OnPropertyChanged();
        }

        //Часть А
        public int FindSingletoneCut()
        {
            List<Vertex> vertices = _graph.Vertices.ToList();
            int minDegree = int.MaxValue;
            for (int i = 0; i < vertices.Count(); i++)
                if (_graph.Degree(vertices[i]) < minDegree)
                    minDegree = _graph.Degree(vertices[i]);
            return minDegree;
        }

        //Часть Б
        public int Ki(int i)
        {
            return (int)Math.Pow(2, i);
        }

        public int FindRootCut(int Ki0, Vertex rootVertex)
        {
            BidirectionalGraph<Vertex, DirectedEdge<Vertex>> subGraph = _graph.Clone();
            List<Vertex> vertices = _graph.Vertices.ToList();
            int i, j;
            for (i = 0; i < subGraph.VertexCount; i++)
            {
                if (subGraph.InDegree(vertices[i]) >= 2 * Ki0 && rootVertex != vertices[i])
                {
                    List<DirectedEdge<Vertex>> inEdges = subGraph.InEdges(vertices[i]).ToList();
                    for (j = 0; j < inEdges.Count; j++)
                        subGraph.AddEdge(new DirectedEdge<Vertex>(inEdges[j].Source, rootVertex));

                    List<DirectedEdge<Vertex>> outEdges = subGraph.OutEdges(vertices[i]).ToList();
                    for (j = 0; j < outEdges.Count; j++)
                        subGraph.AddEdge(new DirectedEdge<Vertex>(rootVertex, outEdges[j].Target));

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

        private int _time = 0;
        public void UtilSCC(Vertex u, Vertex prev, List<int> low, List<int> disc, List<bool> stackMember, Stack<Pair<Vertex, Vertex>> st, List<List<Pair<Vertex, Vertex>>> stronglyConnectedComponents)
        {
            disc[u.ID] = low[u.ID] = _time;
            _time++;
            stackMember[u.ID] = true;
            st.Push(new Pair<Vertex, Vertex>(u, prev));

            foreach (Vertex neighbor in _graph.GetOutNeighbors(_graph.Vertices.ElementAt(u.ID)))
            {
                if (disc[neighbor.ID] == -1)
                {
                    UtilSCC(neighbor, u, low, disc, stackMember, st, stronglyConnectedComponents);
                    low[u.ID] = Math.Min(low[u.ID], low[neighbor.ID]);
                }
                else if (stackMember[neighbor.ID] == true)
                    low[u.ID] = Math.Min(low[u.ID], disc[neighbor.ID]);
            }

            int k = -1;
            if (low[u.ID] == disc[u.ID])
            {
                List<Pair<Vertex, Vertex>> component = new List<Pair<Vertex, Vertex>>();
                while (k != u.ID)
                {
                    component.Add(st.Pop());
                    k = component.Last().First.ID;
                    stackMember[k] = false;
                }
                stronglyConnectedComponents.Add(component);
            }
        }

        public List<List<Pair<Vertex, Vertex>>> TrajanSCC()
        {
            //Будем хранить не только посещенную вершину, но и её предшественника. Если предшественник совпадает с текущей, то эта вершина - начальная.
            List<List<Pair<Vertex, Vertex>>> stronglyConnectedComponents = new List<List<Pair<Vertex, Vertex>>>();
            List<int> disc = new List<int>();
            List<int> low = new List<int>();
            List<bool> stackMember = new List<bool>();
            disc.Capacity = low.Capacity = stackMember.Capacity = _graph.VertexCount;
            for (int i = 0; i < _graph.VertexCount; i++)
            {
                disc.Add(-1);
                low.Add(-1);
                stackMember.Add(false);
            }
            Stack<Pair<Vertex, Vertex>> st = new Stack<Pair<Vertex, Vertex>>();
            for (int i = 0; i < _graph.VertexCount; i++)
            {
                if (disc[i] == -1)
                    UtilSCC(_graph.Vertices.ElementAt(i), _graph.Vertices.ElementAt(i), low, disc, stackMember, st, stronglyConnectedComponents);
            }

            return stronglyConnectedComponents;
        }

        public BidirectionalGraph<Vertex, DirectedEdge<Vertex>> graphFromSCC(List<List<Pair<Vertex, Vertex>>> stronglyConnectedComponents)
        {
            BidirectionalGraph<Vertex, DirectedEdge<Vertex>> treeGraph = new BidirectionalGraph<Vertex, DirectedEdge<Vertex>>();
            for (int i = 0; i < stronglyConnectedComponents.Count; i++)
            {
                treeGraph.AddVertex(stronglyConnectedComponents[i][0].First);
                treeGraph.AddVertex(stronglyConnectedComponents[i][0].Second);
                treeGraph.AddEdge(new DirectedEdge<Vertex>(stronglyConnectedComponents[i][0].Second, stronglyConnectedComponents[i][0].First));
                for (int j = 1; j < stronglyConnectedComponents[i].Count; j++)
                {
                    if (stronglyConnectedComponents[i][j].Second != stronglyConnectedComponents[i][j].First)
                    {
                        if (!treeGraph.ContainsVertex(stronglyConnectedComponents[i][j].First))
                            treeGraph.AddVertex(stronglyConnectedComponents[i][j].First);
                        if (!treeGraph.ContainsVertex(stronglyConnectedComponents[i][j].Second))
                            treeGraph.AddVertex(stronglyConnectedComponents[i][j].Second);
                        treeGraph.AddEdge(new DirectedEdge<Vertex>(stronglyConnectedComponents[i][j].Second, stronglyConnectedComponents[i][j].First));
                    }
                }
            }
            return treeGraph;
        }
        /*
        private BidirectionalGraph<Vertex, SEdge<Vertex>> subGraph;

        Габо иди нахуй))0)
        public int GabowAlgorithm(int k)
        {
            //Создадим копию графа, и граф с обратными вершинами
            subGraph = _graph.Clone();
            BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> reversedGraph = new BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>>();
            reversedGraph.AddVertexRange(subGraph.Vertices);
            List<STaggedEdge<Vertex, int?>> edges = subGraph.Edges.ToList();
            List<Vertex> vertices = subGraph.Vertices.ToList();
            for (int i = 0; i < subGraph.EdgeCount; i++)
                reversedGraph.AddEdge(new STaggedEdge<Vertex, int?>(edges[i].Target, edges[i].Source, null));

            //Создадим мноджестве деревьев Т и заполним его
            List<BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>>> T = new List<BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>>>();
            T.Capacity = k;
            for (int i = 0; i < k - 1; i++)
            {
                BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> tree = new BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>>();
                tree.AddVertex(vertices[i]);
                T.Add(tree);
            }
            //ОТдельно создадим к-ый элемент, состоящий только из вершин.
            BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> treeK = new BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>>();
            treeK.AddVertexRange(vertices);
            T.Add(treeK);

            //Начальная инициализация завершена, начнём алгоритм:
            RoundStep(T, k);
            return 10;
        }

        private int RoundStep(List<BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>>> T, int k)
        {
            if (T[k - 1].EdgeCount + 1 == T[k - 1].VertexCount)
                return (k - 1);
            List<bool> isTreeActive = new List<bool>();
            for (int i = 0; i < k; i++)
            isTreeActive.Add(true);
            AugmentStep(isTreeActive, T, k);
        }

        private void AugmentStep(List<bool> isTreeActive, List<BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>>> T, int k)
        {
            int i = 0;
            while (isTreeActive[i] == true)
            {
                SearchStep(T[i], T[k-1], k);
                i++;
                if (i == k)
                    break;
            }
            AugmentingAlgorithm();
            RoundStep(T, k);
        }
        private int SearchStep(BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> Ti, BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> Tk, int k)
        {
            //Как сделать проверку на два возвращаемых дерева, найденных в алгоритме?
            if (CyclicScanningAlgorithm(Ti, k) == true)
            {
                //Объединить два дерева, объединенные найденным ребром е
                AugmentStep();
            }
            else
            {
                //Сделать услових выхода
                throw new Exception("Забыл сделать условие выхода");
            }
        }

        private bool CyclicScanningAlgorithm(BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> Ti, BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> Tk, int k)
        {
            List<STaggedEdge<Vertex, int?>> Q = new List<STaggedEdge<Vertex, int?>>();
            int i = 1, l = 0;
            List<STaggedEdge<Vertex, int?>> outEdgesOfZ = subGraph.OutEdges(Ti.Vertices.First()).ToList();
            for (int j = 0; j < outEdgesOfZ.Count; j++)
                if (!Tk.ContainsEdge(outEdgesOfZ[j]))
                    LabelStep(l, outEdgesOfZ[j], Q);
            NextEdgeStep(Q, i, Ti);
            
            
        }

        private void LabelStep(int l, STaggedEdge<Vertex, int?> g, List<STaggedEdge<Vertex, int?>> Q)
        {
            if (g.Tag != null)
            {
                throw new Exception("Тогда надо добавить её в путь P и завершиться с успехом");
            }
            else
            {
                g.Tag = l;
                Q.Add(g);
            }
        }

        private void NextEdgeStep(List<STaggedEdge<Vertex, int?>> Q, int i, BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> Ti)
        {
            if (Q.Count == 0)
                throw new Exception("Поиск неудачен");
            STaggedEdge<Vertex, int?> e = Q.First();
            Q.RemoveAt(0);
            if (Ti.ContainsEdge(e))
                i++;
            FundamentalCycleStep(Q, i, Ti, e);
        }

        private void FundamentalCycleStep(List<STaggedEdge<Vertex, int?>> Q, int i, BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> Ti, STaggedEdge<Vertex, int?> e)
        {
            if (Ti.ContainsVertex(e.Source) && Ti.ContainsVertex(e.Target))
                NextEdgeStep(Q, i, Ti);
            else
            {
                CycleTraversalAlgorithm(e, Ti);
                //Начни отсюда
            }
        }

        private void CycleTraversalAlgorithm(STaggedEdge<Vertex, int?> e, BidirectionalGraph<Vertex, STaggedEdge<Vertex, int?>> Ti)
        {
            Vertex u;
            if (!Ti.ContainsVertex(e.Source))
                u = e.Source;
            if (!Ti.ContainsVertex(e.Target))
                u = e.Target;
            Vertex ri = Ti.Vertices.First();


        }

        private int AugmentingAlgorithm()
        { 
            
        }
        */
    }
}
