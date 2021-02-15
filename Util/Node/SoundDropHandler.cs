using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AngelNode.Model.Resource;
using GongSolutions.Wpf.DragDrop;

namespace AngelNode.Util.Node
{
    class SoundDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is File f && f.GuessType() == ResourceType.Sound)
            {
                dropInfo.Effects = DragDropEffects.All;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (((ListBox)dropInfo.VisualTarget).SelectedItem == dropInfo.Data) return;

            var items = (ObservableCollection<File>)((ListBox)dropInfo.VisualTarget).ItemsSource;
            items.Clear();
            items.Add(dropInfo.Data as File);

            ((ListBox)dropInfo.VisualTarget).SelectedItem = dropInfo.Data;
        }
    }
}
