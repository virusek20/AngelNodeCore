﻿using System.Windows;
using System.Windows.Controls;
using AngelNode.Model;
using GongSolutions.Wpf.DragDrop;

namespace AngelNode.Util.Node
{
    public class DialogueNodeDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Character)
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
