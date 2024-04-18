using DimplowTools.Commands;
using DimplowTools.Models;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
using GraphShape.Algorithms.Layout;
using System.Collections.ObjectModel;

namespace DimplowTools.ViewModels
{
    internal class UndirectedGraphVM : ObservableClass
    {
        [DllImport("MCDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern double MinCutStart(int n, int m, int[] edges, double d);

        private string _isGraphConnected;
        public string IsGraphConnected
        {
            get
            {
                return _isGraphConnected;
            }
            set
            {
                _isGraphConnected = value;
                OnPropertyChanged();
            }
        }

        private double _graphMinCut;
        public double GaphMinCut
        {
            get
            {
                return _graphMinCut;
            }
            set
            {
                _graphMinCut = value;
                OnPropertyChanged();
            }
        }
        private BidirectionalGraph<Vertex, SEdge<Vertex>> _graph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
        private UndirectedGraphModel _graphShapeModel;
        public string GraphType { get; set; }
        public BidirectionalGraph<Vertex, SEdge<Vertex>> Graph
        {
            get
            {
                return _graph;
            }
            set
            {
                _graph = value;
                OnPropertyChanged();
            }
        }

        private int _fieldSize = 1000;
        public int FieldSize
        {
            get { return _fieldSize; }
            set { _fieldSize = value; }
        }

        private int _vertexAmount = 20;
        public int VertexAmount
        {
            get { return _vertexAmount; }
            set { _vertexAmount = value; }
        }

        private int _minRadius = 300;
        public int MinRadius
        {
            get { return _minRadius; }
            set { _minRadius = value; }
        }

        private int _maxRadius = 300;
        public int MaxRadius
        {
            get { return _maxRadius; }
            set { _maxRadius = value; }
        }

        private int _oneVertexCut;
        public int OneVertexCut
        {
            get
            {
                return _oneVertexCut;
            }
            set
            {
                _oneVertexCut = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand GenerateGraphCommand { get; set; }
        public RelayCommand FindMinCutCommand { get; set; }
        public RelayCommand DFSCommand { get; set; }
        public ObservableCollection<string> layoutAlgorithms { get; set; }
        private string _currentAlgorithm = "ISOM";
        public string CurrentAlgorithm
        {
            get { return _currentAlgorithm; }
            set
            {
                _currentAlgorithm = value;
                OnPropertyChanged();
            }
        }
        public UndirectedGraphVM()
        {
            StandardLayoutAlgorithmFactory<Vertex, SEdge<Vertex>, BidirectionalGraph<Vertex, SEdge<Vertex>>> algorithmFactory =
                new StandardLayoutAlgorithmFactory<Vertex, SEdge<Vertex>, BidirectionalGraph<Vertex, SEdge<Vertex>>>();
            layoutAlgorithms = new ObservableCollection<string>(algorithmFactory.AlgorithmTypes);

            _graphShapeModel = new UndirectedGraphModel();

            _graphShapeModel.PropertyChanged += (o, s) =>
            {
                Graph = _graphShapeModel.Graph;
            };

            FindMinCutCommand = new RelayCommand(o =>
            {
                OneVertexCut = _graphShapeModel.FindOneVertexCut();
                GaphMinCut = MinCutStart(_graphShapeModel.Graph.VertexCount, _graphShapeModel.Graph.EdgeCount / 2, _graphShapeModel.EdgesForDLL().ToArray(), 1);
            });

            GenerateGraphCommand = new RelayCommand(o =>
            {
                IsGraphConnected = "Нет";
                _graphShapeModel.GenerateUndirectedVertices(VertexAmount, MinRadius, FieldSize);
                _graphShapeModel.GenerateUndirectedEdges();
                if (_graphShapeModel.DFS())
                    IsGraphConnected = "Да";
            });
        }
    }
}
