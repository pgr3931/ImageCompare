using ImageCompare;
using ImageCompare.Model;
using ImageCompareUI.Model;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageCompareUI.ViewModel
{
    public class ComparisonViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ComparisonViewModel()
        {
            Root = IOManager.ReadSetting("root");
            Images = new();
            Images.CollectionChanged += (_, _) =>
            {
                var images = Images.FirstOrDefault();
                CurrentImage = images == null ? null : Tuple.Create(CreateImage(images.Item1), CreateImage(images.Item2));
                Duplicates = Images.Count;
            };
        }

        public ObservableCollection<Tuple<string, string>> Images { get; }
        private bool Running { get; set; }

        private string? _root;
        public string? Root
        {
            get => _root;
            set
            {
                _root = value;
                if (value != null)
                    IOManager.AddOrUpdateSettings("root", value);
                OnPropertyChanged();
            }
        }

        private Tuple<BitmapImage, BitmapImage>? _currentImage;
        public Tuple<BitmapImage, BitmapImage>? CurrentImage
        {
            get { return _currentImage; }
            set
            {
                _currentImage = value;
                DeletePossible = value == null ? Visibility.Hidden : Visibility.Visible;
                OnPropertyChanged();
            }
        }

        private Visibility? _deletePossible;
        public Visibility? DeletePossible
        {
            get { return _deletePossible ?? Visibility.Hidden; }
            set
            {
                _deletePossible = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set
            {
                _elapsedTime = value;
                OnPropertyChanged();
            }
        }

        private int _searchedImages;
        public int SearchedImages
        {
            get { return _searchedImages; }
            set
            {
                _searchedImages = value;
                OnPropertyChanged();
            }
        }

        private int _duplicates;
        public int Duplicates
        {
            get { return _duplicates; }
            set
            {
                _duplicates = value;
                OnPropertyChanged();
            }
        }

        private void Reset()
        {
            ElapsedTime = new TimeSpan();
            Images.Clear();
            SearchedImages = 0;
            Duplicates = 0;
            CurrentImage = null;
        }

        private static BitmapImage CreateImage(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path);
            image.EndInit();
            return image;
        }

        private RelayCommand? _compareCommand;
        public RelayCommand CompareCommand
        {
            get
            {
                return _compareCommand ??= new RelayCommand(
                    async (x) =>
                    {
                        Reset();
                        Running = true;
                        var tokenSource = new CancellationTokenSource();
                        var ct = tokenSource.Token;

                        var task = Task.Run(() =>
                        {
                            while (true)
                            {
                                Thread.Sleep(200);
                                if (ct.IsCancellationRequested)
                                {
                                    break;
                                }
                                Application.Current.Dispatcher.BeginInvoke(delegate
                                {
                                    ElapsedTime = ElapsedTime.Add(new TimeSpan(0, 0, 0, 0, 200));
                                });
                            }
                        }, ct);


                        await Task.Run(() => ImageComparer.Compare(Root!, Images, (x) => SearchedImages = x));

                        tokenSource.Cancel();
                        Running = false;
                        _compareCommand?.RaiseCanExecuteChanged();
                    },
                    (x) => !Running && !string.IsNullOrEmpty(Root) && Directory.Exists(Root));
            }
        }

        private RelayCommand? _deleteCommand;
        public RelayCommand DeleteCommand
        {
            get
            {
                return _deleteCommand ??= new RelayCommand(
                    (x) =>
                    {
                        try
                        {
                            var uri = ((BitmapImage?)x)?.UriSource.OriginalString;
                            Images.RemoveAt(0);
                            if (x != null && File.Exists(uri))
                            {
                                Images.RemoveAt(0);
                                FileSystem.DeleteFile(uri, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                return;
                            }

                            throw new Exception(File.Exists(uri) ? $"File does not exist: {uri}" : "");
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Something went wrong while deleting the file.\n" + e.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    },
                    (x) => true);
            }
        }

        private RelayCommand? _skipCommand;
        public RelayCommand SkipCommand
        {
            get
            {
                return _skipCommand ??= new RelayCommand((x) => Images.RemoveAt(0), (x) => true);
            }
        }
    }
}
