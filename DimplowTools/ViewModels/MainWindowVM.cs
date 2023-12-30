using DimplowTools.Commands;
using DimplowTools.Controls;
using DimplowTools.Models;
using GraphShape.Algorithms.Layout;
using GraphShape.Controls;
using JetBrains.Annotations;
using QuikGraph;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DimplowTools.ViewModels
{
    internal class MainWindowVM : ObservableClass
    {
        private BidirectionalGraph<Vertex, STaggedEdge<Vertex, int>> _graph = new BidirectionalGraph<Vertex, STaggedEdge<Vertex, int>>();
        private GraphShapeModel _graphShapeModel;
        public BidirectionalGraph<Vertex, STaggedEdge<Vertex, int>> Graph
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

        private int _fieldSize = 10000;
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

        private int _minRadius = 100;
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

        public RelayCommand GenerateGraphCommand { get; set; }
        public MainWindowVM()
        {
            _graphShapeModel = GraphShapeModel.GetInstance();

            _graphShapeModel.PropertyChanged += (sender, args) =>
            {
                Graph = _graphShapeModel.Graph;
            };

            GenerateGraphCommand = new RelayCommand(o =>
            {
                _graphShapeModel.GenerateVertices(VertexAmount, MinRadius, MaxRadius, FieldSize);
                _graphShapeModel.GenerateEdges();
            });

        }
    }
}
