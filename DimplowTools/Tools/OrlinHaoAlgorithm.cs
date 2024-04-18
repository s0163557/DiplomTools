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
    internal class OrlinHaoAlgorithm
    {
        List<List<int>> DormantSet = new List<List<int>>();
        int DMax;
        List<int> W = new List<int>();
        List<int> d = new List<int>();
        int SCount = 0;
        List<bool> S = new List<bool>();
        int tStrich;
        int BestValue = int.MaxValue;
        DirectedGraphModel _directedGraphModel;
        public OrlinHaoAlgorithm(DirectedGraphModel directedGraphModel)
        {
            _directedGraphModel = directedGraphModel;
            for (int i = 0; i < _directedGraphModel.Graph.VertexCount; i++)
            {
                W.Add(i);
                d.Add(1);
                S.Add(false);
            }
        }

        public int FindMinCut()
        {
            ModifiedInitialize();
            while (SCount != _directedGraphModel.Graph.VertexCount)
            {
                while (W.Count > 0)
                {
                    foreach (DirectedEdge<Vertex> arc in _directedGraphModel.Graph.OutEdges(_directedGraphModel.Graph.Vertices.ElementAt(W.First())))
                    {
                        if (!W.Contains(arc.Source.ID))
                            break;
                        if (W.Contains(arc.Target.ID) && d[arc.Source.ID] == d[arc.Target.ID] + 1 && arc.IsVisited == false)
                            arc.IsVisited = true;
                        else
                            ModifiedRelabel(arc.Source.ID);
                    }
                }
                int cutCounter = 0;
                List<int> D = new List<int>();
                foreach (List<int> set in DormantSet)
                    D = D.Union(set).ToList();
                foreach (int node in D)
                {
                    foreach (DirectedEdge<Vertex> edge in _directedGraphModel.Graph.OutEdges(_directedGraphModel.Graph.Vertices.ElementAt(node)))
                        if (!D.Contains(edge.Target.ID))
                            cutCounter++;
                }
                if (BestValue > cutCounter)
                    BestValue = cutCounter;
                SelectNewSink();

            }
            return BestValue;
        }

        private void ModifiedInitialize()
        {
            foreach (DirectedEdge<Vertex> edge in _directedGraphModel.Graph.OutEdges(_directedGraphModel.Graph.Vertices.ElementAt(1)))
                edge.IsVisited = true;
            DormantSet.Add(new List<int>());
            DormantSet[0].Add(1);
            DMax = 0;
            W.Remove(1);
            tStrich = 2;
            d[tStrich] = 0;
            //Я их уже сделал единичками в констуркторе, так что живем
        }

        private void ModifiedRelabel(int vertexID)
        {
            bool isOnly = true;
            bool noArc = true;
            int minD = int.MaxValue;
            List<int> R = new List<int>();
            foreach (DirectedEdge<Vertex> arc in _directedGraphModel.Graph.OutEdges(_directedGraphModel.Graph.Vertices.ElementAt(vertexID)))
            {
                if (W.Contains(arc.Target.ID) && arc.IsVisited == false)
                {
                    noArc = false;
                    if (d[arc.Target.ID] + 1 <= minD)
                        minD = d[arc.Target.ID] + 1;
                }
            }
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
                DormantSet[DMax] = new List<int>(vertexID);
                W.Remove(vertexID);
            }
            else
                d[vertexID] = minD;
        }

        public void SelectNewSink()
        {
            W.Remove(tStrich);
            SCount++;
            S[tStrich] = true;
            DormantSet[0].Add(tStrich);
            if (SCount != _directedGraphModel.Graph.VertexCount)
            {
                foreach (DirectedEdge<Vertex> arc in _directedGraphModel.Graph.Edges)
                {
                    if (!S[arc.Target.ID])
                        arc.IsVisited = true;
                }
            }
            if (W.Count == 0)
            {
                W = DormantSet[DMax];
                DMax--;
            }
            int minD = int.MaxValue;
            foreach (int node in W)
                if (d[node] < minD)
                    minD = d[node];
            tStrich = minD;
        }
    }
}
