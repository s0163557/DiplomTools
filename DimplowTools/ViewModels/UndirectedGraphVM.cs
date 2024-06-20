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
using System.Threading;
using System.Windows.Media;
using Microsoft.Win32;

namespace DimplowTools.ViewModels
{
    internal class UndirectedGraphVM : ObservableClass
    {
        private string _isGraphConnected = "Нет";
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
        public double GraphMinCut
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
        private BidirectionalGraph<Vertex, SEdge<Vertex>> _prevGraph = new BidirectionalGraph<Vertex, SEdge<Vertex>>();
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

        private int _radius = 300;
        public int Radius
        {
            get { return _radius; }
            set { _radius = value; }
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

        private int _sinkVertexNumber;
        public int SinkVertexNumber
        {
            get
            {
                return _sinkVertexNumber;
            }
            set
            {
                _sinkVertexNumber = value;
                OnPropertyChanged();
            }
        }

        private int _graphAmount = 1000;
        public int GraphAmount
        {
            get
            {
                return _graphAmount;
            }
            set
            {
                _graphAmount = value;
                OnPropertyChanged();
            }
        }

        private int _moveMinRadius = 10;
        public int MoveMinRadius
        {
            get
            {
                return _moveMinRadius;
            }
            set
            {
                _moveMinRadius = value;
                OnPropertyChanged();
            }
        }

        private int _moveMaxRadius = 50;
        public int MoveMaxRadius
        {
            get
            {
                return _moveMaxRadius;
            }
            set
            {
                _moveMaxRadius = value;
                OnPropertyChanged();
            }
        }

        private int _moveAmount = 100;
        public int MoveAmount
        {
            get
            {
                return _moveAmount;
            }
            set
            {
                _moveAmount = value;
                OnPropertyChanged();
            }
        }

        private int _moveGraphAmount = 1000;
        public int MoveGraphAmount
        {
            get
            {
                return _moveGraphAmount;
            }
            set
            {
                _moveGraphAmount = value;
                OnPropertyChanged();
            }
        }

        private int _cfAmount = 10000;
        public int CFAmount
        {
            get
            {
                return _cfAmount;
            }
            set
            {
                _cfAmount = value;
                OnPropertyChanged();
            }
        }
        private bool[] _graphDontExistErrors = new bool[7];
        public bool[] GraphDontExistErrors
        {
            get
            {
                return _graphDontExistErrors;
            }
            set
            {
                _graphDontExistErrors = value;
                OnPropertyChanged();
            }
        }
        public void ErrorOutput(int id)
        {
            GraphDontExistErrors[id] = true;
            OnPropertyChanged(nameof(GraphDontExistErrors));
            Task task1 = new Task(() =>
            {
                Thread.Sleep(5000);
                GraphDontExistErrors[id] = false;
                OnPropertyChanged(nameof(GraphDontExistErrors));
            });
            task1.Start();
        }

        public RelayCommand GenerateGraphCommand { get; set; }
        public RelayCommand FindMinCutCommand { get; set; }
        public RelayCommand DFSCommand { get; set; }
        public RelayCommand SinkCommand { get; set; }
        public RelayCommand MoveVerticesCommand { get; set; }
        public RelayCommand PreviousGraphCommand { get; set; }
        public RelayCommand ConnectivityFunctionCommand { get; set; }
        public RelayCommand GraphResearchCommand { get; set; }
        public RelayCommand MoveResearchCommand { get; set; }
        public RelayCommand MoveResearchDirectionsCommand { get; set; }
        public RelayCommand CFResearchCommand { get; set; }
        public RelayCommand SinkMouseCapture { get; set; }
        public RelayCommand MRPSavePathCommand { get; set; }
        public RelayCommand MRDSavePathCommand { get; set; }
        public RelayCommand CFSavePathCommand { get; set; }
        public RelayCommand CFRSavePathCommand { get; set; }
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

        public static Dictionary<string, Brush> ControlsColors { get; set; }
        public static class ColorBehaviour
        {

            public static void SinkChangeColorGreen()
            {
                ControlsColors["SinkButton"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGreen"));
            }

        }
        private string _UPResultfilepath = System.AppDomain.CurrentDomain.BaseDirectory + "UPResearchResult.txt";
        public string UPResultfilepath
        {
            get
            {
                return _UPResultfilepath;
            }
            set
            {
                _UPResultfilepath = value;
                OnPropertyChanged();
            }
        }

        private string _UDResultfilepath = System.AppDomain.CurrentDomain.BaseDirectory + "UDResearchResult.txt";
        public string UDResultfilepath
        {
            get
            {
                return _UDResultfilepath;
            }
            set
            {
                _UDResultfilepath = value;
                OnPropertyChanged();
            }
        }

        private string _CFfilepath = System.AppDomain.CurrentDomain.BaseDirectory + "CFResult.txt";
        public string CFfilepath
        {
            get
            {
                return _CFfilepath;
            }
            set
            {
                _CFfilepath = value;
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
        public UndirectedGraphVM()
        {
            StandardLayoutAlgorithmFactory<Vertex, SEdge<Vertex>, BidirectionalGraph<Vertex, SEdge<Vertex>>> algorithmFactory =
                new StandardLayoutAlgorithmFactory<Vertex, SEdge<Vertex>, BidirectionalGraph<Vertex, SEdge<Vertex>>>();
            layoutAlgorithms = new ObservableCollection<string>(algorithmFactory.AlgorithmTypes);

            ControlsColors = new Dictionary<string, Brush>
            {
                {"SinkButton", new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"))}
            };

            _graphShapeModel = new UndirectedGraphModel();

            FindMinCutCommand = new RelayCommand(o =>
            {
                if (IsGraphConnected != "Да")
                    ErrorOutput(5);
                else GraphMinCut = _graphShapeModel.KargerSteinAlgorithm(Graph) / 2;
            });

            GenerateGraphCommand = new RelayCommand(o =>
            {
                IsGraphConnected = "Нет";
                Graph = _graphShapeModel.GenerateUndirectedEdges(_graphShapeModel.GenerateUndirectedVertices(VertexAmount, Radius, FieldSize));
                if (_graphShapeModel.DFS(Graph))
                    IsGraphConnected = "Да";
                else
                    IsGraphConnected = "Нет";
            });

            SinkCommand = new RelayCommand(o =>
            {
                if (Graph.VertexCount == 0)
                    ErrorOutput(0);
                else if (SinkVertexNumber < 0 || SinkVertexNumber > Graph.VertexCount)
                    ErrorOutput(4);
                else
                    Graph = _graphShapeModel.FindSinkCuts(Graph, SinkVertexNumber);
            });

            MoveVerticesCommand = new RelayCommand(o =>
            {
                if (Graph.VertexCount == 0)
                {
                    ErrorOutput(1);
                }
                else
                {
                    Graph = _graphShapeModel.MoveVertices(Graph, FieldSize, MoveMinRadius, MoveMaxRadius);
                    if (_graphShapeModel.DFS(Graph))
                        IsGraphConnected = "Да";
                    else
                        IsGraphConnected = "Нет";
                }
            });

            PreviousGraphCommand = new RelayCommand(o =>
            {
                Graph = _prevGraph;
            });

            ConnectivityFunctionCommand = new RelayCommand(o =>
            {
                if (IsGraphConnected != "Да")
                    ErrorOutput(6);
                else
                    _graphShapeModel.FindConnectivityFunction(Graph, FieldSize, CFfilepath);
            });

            GraphResearchCommand = new RelayCommand(o =>
            {
                _graphShapeModel.ConnectivityResearch(_vertexAmount, Radius, _fieldSize, GraphAmount, CFRfilepath);
            });

            MoveResearchCommand = new RelayCommand(o =>
            {
                if (MoveGraphAmount % 5 != 0)
                {
                    ErrorOutput(2);
                }
                else
                    _graphShapeModel.MoveResearchPoints(VertexAmount, Radius, MoveGraphAmount, MoveAmount, MoveMinRadius, MoveMaxRadius, FieldSize, UPResultfilepath);
            });

            MoveResearchDirectionsCommand = new RelayCommand(o =>
            {
                if (MoveGraphAmount % 5 != 0)
                {
                    ErrorOutput(3);
                }
                else
                    _graphShapeModel.MoveResearchDirections(VertexAmount, Radius, MoveGraphAmount, MoveAmount, MoveMinRadius, MoveMaxRadius, FieldSize, UDResultfilepath);
            });

            MRPSavePathCommand = new RelayCommand(o =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = "UPResearchResult";
                openFileDialog.DefaultExt = ".txt";
                openFileDialog.Filter = "Text documents (.txt)|*.txt";
                var result = openFileDialog.ShowDialog();
                if (result == true)
                    UPResultfilepath = openFileDialog.FileName;
            });

            MRDSavePathCommand = new RelayCommand(o =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = "UDResearchResult";
                openFileDialog.DefaultExt = ".txt";
                openFileDialog.Filter = "Text documents (.txt)|*.txt";
                var result = openFileDialog.ShowDialog();
                if (result == true)
                    UDResultfilepath = openFileDialog.FileName;
            });

            CFSavePathCommand = new RelayCommand(o =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = "ConnectivityFunctionResult";
                openFileDialog.DefaultExt = ".txt";
                openFileDialog.Filter = "Text documents (.txt)|*.txt";
                var result = openFileDialog.ShowDialog();
                if (result == true)
                    CFfilepath = openFileDialog.FileName;
            });

            CFRSavePathCommand = new RelayCommand(o =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = "CFResearchResult";
                openFileDialog.DefaultExt = ".txt";
                openFileDialog.Filter = "Text documents (.txt)|*.txt";
                var result = openFileDialog.ShowDialog();
                if (result == true)
                    CFRfilepath = openFileDialog.FileName;
            });
        }
    }
}