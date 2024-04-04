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
        public RelayCommand OpenDirectedGraphView { get; set; }
        public RelayCommand OpenUndirectedGraphView { get; set; }

        public DirectedGraphVM DirectedGraphVM { get; set; }
        public UndirectedGraphVM UndirectedGraphVM { get; set; }

        private object _viewModel = null;
        public object CurrentVM
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnPropertyChanged();
            }
        }

        public MainWindowVM()
        { 
            DirectedGraphVM = new DirectedGraphVM();
            UndirectedGraphVM = new UndirectedGraphVM();
            CurrentVM = DirectedGraphVM;
            OpenDirectedGraphView = new RelayCommand((o) => 
            {
                CurrentVM = DirectedGraphVM;
                OnPropertyChanged();
            });
            OpenUndirectedGraphView = new RelayCommand((o) =>
            {
                CurrentVM = UndirectedGraphVM;
                OnPropertyChanged();
            });
        }
        
    }
}
