using System.Windows;

namespace AngelNode.View
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();

            Application.Current.MainWindow = this;
        }
    }
}
