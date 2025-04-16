using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;
using System.Windows;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        #region ctor

        private int _numberOfBalls;
        public int NumberOfBalls
        {
            get => _numberOfBalls;
            set
            {
                if (_numberOfBalls != value)
                {
                    _numberOfBalls = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand StartCommand { get; }

        public MainWindowViewModel()
        {
            StartCommand = new RelayCommand(() => Start(NumberOfBalls));
            ModelLayer = ModelAbstractApi.CreateModel();
            Observer = ModelLayer.Subscribe(ball => 
            {
                Balls.Add(ball);
            });
        }

        public MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            StartCommand = new RelayCommand(() => Start(NumberOfBalls));
            ModelLayer = modelLayerAPI ?? ModelAbstractApi.CreateModel();
        }

        #endregion ctor

        #region public API

        public void Start(int numberOfBalls)
        {
            Console.WriteLine("MainWindowViewModel Start called.");
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Balls.Clear(); // Clear the collection before starting - but if we want the functionality of adding new balls, we can remove this line.
            ModelLayer.Start(numberOfBalls);
        }
        

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer?.Dispose();
                    ModelLayer?.Dispose();
                }

                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

        #endregion private
    }
}