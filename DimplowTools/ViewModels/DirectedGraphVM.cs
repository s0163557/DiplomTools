using DimplowTools.Commands;
using DimplowTools.Data;
using DimplowTools.Models;
using DimplowTools.Tools;
using GraphShape.Algorithms.Layout;
using Microsoft.Win32;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DimplowTools.ViewModels
{
    internal class DirectedGraphVM : ObservableClass
    {
        public ObservableCollection<string> layoutAlgorithms { get; set; }
        private BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> _graph = new BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>>();
        private BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> _prevGraph = new BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>>();
        private DirectedGraphModel _graphShapeModel;
        public string GraphType { get; set; }
        public BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> Graph
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

        private bool _graphDontExistError = false;
        public bool GraphDontExistError
        {
            get { return _graphDontExistError; }
            set { 
                _graphDontExistError = value;
                OnPropertyChanged();       
                }
        }

        private bool _multiplisityError = false;
        public bool MultiplisityError
        {
            get { return _multiplisityError; }
            set
            {
                _multiplisityError = value;
                OnPropertyChanged();
            }
        }
        private bool _multiplisityError2 = false;
        public bool MultiplisityError2
        {
            get { return _multiplisityError; }
            set
            {
                _multiplisityError = value;
                OnPropertyChanged();
            }
        }
        private int _vertexAmount = 30;
        public int VertexAmount
        {
            get { return _vertexAmount; }
            set { _vertexAmount = value; }
        }

        private int _minRadius = 250;
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

        private int _moveMinRadius = 10;
        public int MoveMinRadius
        {
            get { return _moveMinRadius; }
            set { _moveMinRadius = value; }
        }

        private int _moveMaxRadius = 50;
        public int MoveMaxRadius
        {
            get { return _moveMaxRadius; }
            set { _moveMaxRadius = value; }
        }

        private int _minWeight = 1;
        public int MinWeight
        {
            get { return _minWeight; }
            set { _minWeight = value; }
        }

        private int _maxWeight = 10;
        public int MaxWeight
        {
            get { return _maxWeight; }
            set { _maxWeight = value; }
        }
        private int _graphAmount = 1000;
        public int GraphAmount
        {
            get { return _graphAmount; }
            set { _graphAmount = value; }
        }
        private int _moveAmount = 10;
        public int MoveAmount
        {
            get { return _moveAmount; }
            set { _moveAmount = value; }
        }
        private int _moveGraphAmount = 1000;
        public int MoveGraphAmount
        {
            get { return _moveGraphAmount; }
            set { _moveGraphAmount = value; }
        }

        private int _graphCutValue = int.MaxValue;
        public int GraphCutValue
        {
            get { return _graphCutValue; }
            set
            {
                _graphCutValue = value;
                OnPropertyChanged();
            }
        }
        private int _maxFlow = int.MaxValue;
        public int MaxFlow
        {
            get
            {
                return _maxFlow;
            }
            set
            {
                _maxFlow = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand GenerateGraphCommand { get; set; }
        public RelayCommand SomeTestCommand { get; set; }
        public RelayCommand SSCCommand { get; set; }
        public RelayCommand PrevGraphCommand { get; set; }
        public RelayCommand GraphCutCommand { get; set; }
        public RelayCommand GraphResearchCommand { get; set; }
        public RelayCommand MoveVerticesCommand { get; set; }
        public RelayCommand MovePointsResearchCommand { get; set; }
        public RelayCommand MoveDirectionsResearchCommand { get; set; }
        public RelayCommand CFRSavePathCommand { get; set; }
        public RelayCommand MPRSavePathCommand { get; set; }
        public RelayCommand MDRSavePathCommand { get; set; }

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

        private string _CFRfilepath = System.AppDomain.CurrentDomain.BaseDirectory + "CFResearchResult.txt";
        public string CFRfilepath
        {
            get
            {
                return _CFRfilepath;
            }
            set
            {
                _CFRfilepath = value;
                OnPropertyChanged();
            }
        }

        private string _DPResultfilepath = System.AppDomain.CurrentDomain.BaseDirectory + "DPResearchResult.txt";
        public string DPResultfilepath
        {
            get
            {
                return _DPResultfilepath;
            }
            set
            {
                _DPResultfilepath = value;
                OnPropertyChanged();
            }
        }

        private string _DDResultfilepath = System.AppDomain.CurrentDomain.BaseDirectory + "DDResearchResult.txt";
        public string DDResultfilepath
        {
            get
            {
                return _DDResultfilepath;
            }
            set
            {
                _DDResultfilepath = value;
                OnPropertyChanged();
            }
        }
        public string OpenFilePathDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.FileName = "CFResearchResult";
            openFileDialog.DefaultExt = ".txt";
            openFileDialog.Filter = "Text documents (.txt)|*.txt";
            var result = openFileDialog.ShowDialog();
            if (result == true)
                return openFileDialog.FileName;
            else
                return null;
        }

        public DirectedGraphVM()
        {
            StandardLayoutAlgorithmFactory<Vertex, DirectedEdge<Vertex>, BidirectionalGraph<Vertex, DirectedEdge<Vertex>>> algorithmFactory =
                new StandardLayoutAlgorithmFactory<Vertex, DirectedEdge<Vertex>, BidirectionalGraph<Vertex, DirectedEdge<Vertex>>>();
            layoutAlgorithms = new ObservableCollection<string>(algorithmFactory.AlgorithmTypes);

            _graphShapeModel = new DirectedGraphModel();

            SomeTestCommand = new RelayCommand(o =>
            {
                Graph = _graphShapeModel.FindSinkCuts(_graphShapeModel.Graph);
            });

            GraphCutCommand = new RelayCommand(async o =>
            {
                GraphDontExistError = false;
                if (Graph.VertexCount == 0)
                {
                    GraphDontExistError = true;
                    Task task1 = new Task(() =>
                    {
                        Thread.Sleep(5000);
                        GraphDontExistError = false;
                    });
                    task1.Start();
                }
                else
                {
                    MaxFlow = int.MaxValue;
                    GraphCutValue = int.MaxValue;
                    Random random = new Random();
                    Task task = new Task(() =>
                    {
                        BidirectionalGraph<WeightedVertex, DirectedEdge<WeightedVertex>> subgraph = Graph.Clone();
                        WeightedOHAlgorithm weightedOHAlgorithm = new WeightedOHAlgorithm(subgraph, random.Next(0, Graph.VertexCount));
                        MaxFlow = Math.Min(weightedOHAlgorithm.FindMinCut(), MaxFlow);
                        GraphCutValue = Math.Min(weightedOHAlgorithm.MinCutAmount, GraphCutValue);

                    });
                    task.Start();
                    await task.ConfigureAwait(false);
                    OnPropertyChanged();
                }
            });

            SSCCommand = new RelayCommand(o =>
            {
                Graph = _graphShapeModel.graphFromSCC(_graphShapeModel.TrajanSCC(Graph));
            });

            PrevGraphCommand = new RelayCommand(o =>
            {
                Graph = _prevGraph;
                _graphShapeModel.Graph = Graph;
            });

            GenerateGraphCommand = new RelayCommand(o =>
            {
                _graphShapeModel.CreateField(FieldSize);
                _graphShapeModel.Graph = _graphShapeModel.GenerateDirectedVertices(VertexAmount, MinRadius, MaxRadius, _graphShapeModel.CreateField(FieldSize), MinWeight, MaxWeight);
                _graphShapeModel.Graph = _graphShapeModel.GenerateDirectedEdges(_graphShapeModel.Graph);
                Graph = _graphShapeModel.Graph;
            });

            GraphResearchCommand = new RelayCommand(o =>
            {
                MultiplisityError2 = false;
                if (GraphAmount % 5 != 0)
                {
                    MultiplisityError2 = true;
                    Task task1 = new Task(() =>
                    {
                        Thread.Sleep(5000);
                        MultiplisityError2 = false;
                    });
                    task1.Start();
                }
                else
                {
                    _graphShapeModel.MultiThreadGraphResearch(VertexAmount, MinRadius, MaxRadius, MinWeight, MaxWeight, GraphAmount, CFRfilepath, FieldSize);
                }
            });

            MoveVerticesCommand = new RelayCommand(o =>
            {
                Graph = _graphShapeModel.MoveVertices(Graph, FieldSize, MoveMinRadius, MoveMaxRadius);
            });

            MovePointsResearchCommand = new RelayCommand(o =>
            {
            MultiplisityError = false;
                if (MoveGraphAmount % 5 != 0)
                {
                    MultiplisityError = true;
                    Task task1 = new Task(() =>
                    {
                        Thread.Sleep(5000);
                        MultiplisityError = false;
                    });
                    task1.Start();
                }
                else
                {
                    _graphShapeModel.MoveResearchPoints(VertexAmount, MinRadius, MaxRadius, MinWeight, MaxWeight, GraphAmount, MoveAmount, MoveMinRadius, MoveMaxRadius, FieldSize, DPResultfilepath);
                }
            });

            MoveDirectionsResearchCommand = new RelayCommand(o =>
            {
            MultiplisityError = false;
                if (MoveGraphAmount % 5 != 0)
                {
                    MultiplisityError = true;
                    Task task1 = new Task(() =>
                    {
                        Thread.Sleep(5000);
                        MultiplisityError = false;
                    });
                    task1.Start();
                }
                else
                {
                    _graphShapeModel.MoveResearchDirections(VertexAmount, GraphAmount, MoveAmount, MoveMinRadius, MoveMaxRadius, FieldSize, MinRadius, MaxRadius, MinWeight, MaxWeight, DDResultfilepath);
                }
            });

            CFRSavePathCommand = new RelayCommand(o =>
            {
                string result = OpenFilePathDialog();
                CFRfilepath = result  == null? CFRfilepath : result;
            });

            MPRSavePathCommand = new RelayCommand(o =>
            {
                string result = OpenFilePathDialog();
                DPResultfilepath = result == null ? CFRfilepath : result;
            });

            MDRSavePathCommand = new RelayCommand(o =>
            {
                string result = OpenFilePathDialog();
                DDResultfilepath = result == null ? CFRfilepath : result;
            });
        }

    }
}
