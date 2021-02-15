using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AngelNode.Model.Resource;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace AngelNode.ViewModel
{
    public class ImagePreviewViewModel : ViewModelBase
    {
        private File _file;
        private ImageSource _image;

        public File File
        {
            get => _file;
            set
            {
                Set(() => File, ref _file, value);
                Set(() => Image, ref _image, new BitmapImage(new Uri(value.Path)));

                RaisePropertyChanged(nameof(ImageWidth));
                RaisePropertyChanged(nameof(ImageHeight));
            }
        }

        public ImageSource Image => _image;
        public double ImageWidth => Image?.Width ?? 0 / 2.0;
        public double ImageHeight => Image?.Height ?? 0 / 2.0;

        public RelayCommand<Window> CloseCommand { get; }

        public ImagePreviewViewModel()
        {
            CloseCommand = new RelayCommand<Window>(Close);
        }

        private void Close(Window window)
        {
            window.Close();
        }
    }
}
