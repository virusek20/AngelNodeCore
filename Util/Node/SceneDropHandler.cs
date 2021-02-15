using AngelNode.Model;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Windows.Controls;

namespace AngelNode.Util.Node
{
    class SceneDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Scene)
            {
                dropInfo.Effects = DragDropEffects.All;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (((ComboBox)dropInfo.VisualTarget).SelectedItem == dropInfo.Data) return;
            ((ComboBox)dropInfo.VisualTarget).SelectedItem = dropInfo.Data;
        }
    }
}
