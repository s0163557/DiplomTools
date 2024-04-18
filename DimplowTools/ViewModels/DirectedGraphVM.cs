using DimplowTools.Commands;
using DimplowTools.Data;
using DimplowTools.Models;
using DimplowTools.Tools;
using GraphShape.Algorithms.Layout;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DimplowTools.ViewModels
{
    internal class DirectedGraphVM : ObservableClass
    {
        public ObservableCollection<string> layoutAlgorithms { get; set; }
        private BidirectionalGraph<Vertex, DirectedEdge<Vertex>> _graph = new BidirectionalGraph<Vertex, DirectedEdge<Vertex>>();
        private BidirectionalGraph<Vertex, DirectedEdge<Vertex>> _prevGraph = new BidirectionalGraph<Vertex, DirectedEdge<Vertex>>();
        private DirectedGraphModel _graphShapeModel;
        public string GraphType { get; set; }
        public BidirectionalGraph<Vertex, DirectedEdge<Vertex>> Graph
        {
            get
            {
                return _graph;
            }
            set
            {
                _prevGraph = _graph;
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

        private int _vertexAmount = 10;
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

        private int _maxRadius = 500;
        public int MaxRadius
        {
            get { return _maxRadius; }
            set { _maxRadius = value; }
        }

        private int _graphCutValue = 0;
        public int GraphCutValue
        {
            get { return _graphCutValue; }
            set { 
                _graphCutValue = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand GenerateGraphCommand { get; set; }
        public RelayCommand SomeTestCommand { get; set; }
        public RelayCommand SSCCommand { get; set; }
        public RelayCommand PrevGraphCommand { get; set; }
        public RelayCommand GraphCutCommand { get; set; }
        private string _currentAlgorithm = "ISOM";
        public string CurrentAlgorithm 
        { 
            get { return _currentAlgorithm; } 
            set { 
                _currentAlgorithm = value;
                OnPropertyChanged();
            } 
        }
        public DirectedGraphVM()
        {
            StandardLayoutAlgorithmFactory<Vertex, DirectedEdge<Vertex>, BidirectionalGraph<Vertex, DirectedEdge<Vertex>>> algorithmFactory = 
                new StandardLayoutAlgorithmFactory<Vertex, DirectedEdge<Vertex>, BidirectionalGraph<Vertex, DirectedEdge<Vertex>>>();
            layoutAlgorithms = new ObservableCollection<string>(algorithmFactory.AlgorithmTypes);
            
            _graphShapeModel = new DirectedGraphModel();

            _graphShapeModel.PropertyChanged += (o, s) =>
            {
                Graph = _graphShapeModel.Graph;
            };

            SomeTestCommand = new RelayCommand(o =>
            {
                _graphShapeModel.FindSinkCuts();
            });

            GraphCutCommand = new RelayCommand(o =>
            {
                OrlinHaoAlgorithm orlinHaoAlgorithm = new OrlinHaoAlgorithm(_graphShapeModel);
                GraphCutValue = orlinHaoAlgorithm.FindMinCut();
            });

            SSCCommand = new RelayCommand(o =>
            {
                Graph = _graphShapeModel.graphFromSCC(_graphShapeModel.TrajanSCC());
            });

            PrevGraphCommand = new RelayCommand(o =>
            {
                Graph = _prevGraph;
            });

            GenerateGraphCommand = new RelayCommand(o =>
            {
                _graphShapeModel.GenerateDirectedVertices(VertexAmount, MinRadius, MaxRadius, FieldSize);
                _graphShapeModel.GenerateDirectedEdges();
            });

        }

    }
}
