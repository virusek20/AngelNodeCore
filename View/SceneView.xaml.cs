using System.Windows.Controls;

namespace AngelNode.View
{
    /// <summary>
    /// Interakční logika pro SceneView.xaml
    /// </summary>
    public partial class SceneView : TabItem
    {
        public SceneView()
        {
            InitializeComponent();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            ((ListBox)sender).ScrollIntoView(e.AddedItems[0]);
        }
    }
}
