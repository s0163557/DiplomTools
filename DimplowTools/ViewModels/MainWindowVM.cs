using DimplowTools.Commands;
using DimplowTools.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DimplowTools.ViewModels
{
    internal class MainWindowVM : ObservableClass
    {
        public RelayCommand GenerateGraphCommand { get; set; }
        private ObservableCollection<Vertex> _vertices;
        public ObservableCollection<Vertex> Vertices 
        {
            get
            {
                return _vertices;
            }
            set
            {
                _vertices = value;
                OnPropertyChanged();
            }
        }
        public Graph CurrentGraph;
        public int CanvasHeight { get; set; } = 600;
        public int CanvasWidth { get; set; } = 900;
        public MainWindowVM()
        {
            CurrentGraph = Graph.GetInstanse();
            GenerateGraphCommand = new RelayCommand(o =>
               {
                   CurrentGraph.GenerateVertices(CanvasHeight, CanvasWidth, 30, 50, 40);
               });

            CurrentGraph.PropertyChanged += (sender, args) =>
            {
                Vertices = CurrentGraph.Vertices;
            };
        }
    }
}
