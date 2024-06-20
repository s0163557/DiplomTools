using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DimplowTools.Tools
{
    internal class ResearchResult
    {
        public int MinCut;
        public int MaxFlow;
        public ResearchResult(int minCut, int maxFlow)
        {
            MinCut = minCut;
            MaxFlow = maxFlow;
        }

        public ResearchResult() { }

        public override string ToString()
        {
            return MinCut + "/" + MaxFlow;
        }
    }
}
