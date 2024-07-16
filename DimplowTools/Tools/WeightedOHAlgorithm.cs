using DimplowTools.Data;
using DimplowTools.Models;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Tools
{
    internal class WeightedOHAlgorithm
    {
        List<List<int>> DormantSet;
        int DMax;
        List<int> W;
        List<int> d;
        int SCount = 0;
        List<bool> S;
        int tStrich;
        public int BestValue;
        public int MinCutAmount;
        int _startVertex;
        BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> _graph;
        public WeightedOHAlgorithm(BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> graph, int startVertex)
        {
            DormantSet = new List<List<int>>();
            W = new List<int>();
            d = new List<int>();
            SCount = 0;
            S = new List<bool>();
            BestValue = int.MaxValue;
            MinCutAmount = int.MaxValue;
            _graph = graph;
            _startVertex = startVertex;

            for (int i = 0; i < _graph.VertexCount; i++)
            {
                W.Add(i);
                d.Add(1);
                S.Add(false);
            }

            foreach (DirectedEdge<WeightedVertex> arc in _graph.Edges)
                arc.OccupiedWeight = 0;

        }

        public int FindMinCut()
        {
            ModifiedInitialize();
            while (SCount != _graph.VertexCount)
            {
                while (W.Count > 0)
                {
                    foreach (DirectedEdge<WeightedVertex> arc in _graph.OutEdges(_graph.Vertices.ElementAt(W.First())))
                    {
                        if (W.Contains(arc.Source.ID) && W.Contains(arc.Target.ID) && d[arc.Source.ID] == d[arc.Target.ID] + 1)
                            arc.OccupiedWeight += Math.Min(e(_graph.Vertices.ElementAt(W.First())), r(arc));
                    }
                    ModifiedRelabel(W.First());
                }

                for (int i = 0; i < DormantSet.Count; i++)
                {
                    int cutSize = 0;
                    int cutCounter = 0;
                    foreach (int node in DormantSet[i])
                    {
                        foreach (DirectedEdge<WeightedVertex> edge in _graph.OutEdges(_graph.Vertices.ElementAt(node)))
                            if (!DormantSet[i].Contains(edge.Target.ID))
                            {
                                cutSize += edge.OccupiedWeight > edge.Weight ? edge.Weight : edge.OccupiedWeight;
                                cutCounter++;
                            }
                    }

                    if (BestValue > cutSize && cutSize > 0)
                        BestValue = cutSize;

                    if (MinCutAmount > cutCounter && cutCounter > 0)
                        MinCutAmount = cutCounter;
                }
                //Не забудь, попробуй провести исследование с этим параметром и без него
                if (SelectNewSink() == 1)
                    return BestValue;
            }
            return BestValue;
        }

        private void ModifiedInitialize()
        {
            Random random = new Random();
            foreach (DirectedEdge<WeightedVertex> edge in _graph.OutEdges(_graph.Vertices.ElementAt(_startVertex)))
                edge.OccupiedWeight = r(edge);
            DormantSet.Add(new List<int>());
            DormantSet[0].Add(_startVertex);
            DMax = 0;
            W.Remove(_startVertex);
            while(tStrich == _startVertex)
                tStrich = random.Next(0, _graph.VertexCount);
            d[tStrich] = 0;
            //Я их уже сделал единичками в констуркторе, так что живем
        }

        private void ModifiedRelabel(int vertexID)
        {
            bool isOnly = true;
            bool noArc = true;
            int minD = int.MaxValue;
            List<int> R = new List<int>();

            for (int i = 0; i < W.Count; i++)
            {
                if (W[i] != vertexID && d[W[i]] == d[vertexID])
                {
                    isOnly = false;
                    break;
                }
                if (d[W[i]] >= d[vertexID])
                    R.Add(W[i]);
            }

            foreach (DirectedEdge<WeightedVertex> arc in _graph.OutEdges(_graph.Vertices.ElementAt(vertexID)))
            {
                if (W.Contains(arc.Target.ID))
                {
                    noArc = false;
                    if (arc.IsVisited == false)
                    {
                        //Поидее 0 будет только в стоке, а на него нам смотреть не надо(поидее)
                        if (d[arc.Target.ID] + 1 <= minD && r(arc) > 0 && d[arc.Target.ID] != 0)
                            minD = d[arc.Target.ID] + 1;
                    }
                }
            }

            if (isOnly)
            {
                DMax++;
                while (DormantSet.Count <= DMax)
                    DormantSet.Add(new List<int>());
                DormantSet[DMax] = R;
                W = W.Except(R).ToList();
            }
            else if (noArc)
            {
                DMax++;
                while (DormantSet.Count <= DMax)
                    DormantSet.Add(new List<int>());
                DormantSet[DMax] = new List<int> { vertexID };
                W.Remove(vertexID);
            }
            else
                d[vertexID] = minD;
        }

        public int SelectNewSink()
        {
            W.Remove(tStrich);
            SCount++;
            if (!S[tStrich])
            {
                S[tStrich] = true;
                SCount++;
            }
            DormantSet[0].Add(tStrich);
            if (SCount != _graph.VertexCount)
            {
                foreach (DirectedEdge<WeightedVertex> arc in _graph.OutEdges(_graph.Vertices.ElementAt(tStrich)))
                    if (!S[arc.Target.ID])
                        arc.OccupiedWeight += r(arc);
            }
            //Этот елсе нужен просто как выход из метода.
            else
                return 2;

            if (W.Count == 0)
            {
                W = DormantSet[DMax];
                DMax--;
            }
            int minD = int.MaxValue;
            foreach (int node in W)
                if (d[node] < minD)
                {
                    minD = d[node];
                    tStrich = node;
                }
            //Возвращаемое значение - пустышка, не морочь себе голову
            //Может быть в этом и проблема? Не понять, там quit означает выход из метода, или из всего алгоритма.
            return 1;
        }

        private int r(DirectedEdge<WeightedVertex> arc)
        {
            int reverseWeight = 0;
            foreach (DirectedEdge<WeightedVertex> reverseArc in _graph.OutEdges(arc.Target))
            {
                if (reverseArc.Target == arc.Source)
                {
                    reverseWeight = reverseArc.OccupiedWeight;
                    break;
                }
            }
            return arc.Weight - arc.OccupiedWeight + reverseWeight;
        }

        private int e(WeightedVertex i)
        {
            int inSumm = 0, outSumm = 0;

            foreach (DirectedEdge<WeightedVertex> arc in _graph.InEdges(i))
                inSumm += arc.Weight;

            foreach (DirectedEdge<WeightedVertex> arc in _graph.OutEdges(i))
                outSumm += arc.Weight;

            return inSumm - outSumm;
        }
    }


}
