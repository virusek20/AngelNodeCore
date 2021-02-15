using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AngelNode.Model.Node;
using GongSolutions.Wpf.DragDrop;

namespace AngelNode.Util.Node
{
    class NodeLibraryDragHandler : IDragSource
    {
        private Type GetListViewType(ListViewItem nodeType)
        {
            var type = nodeType.Resources?["NodeType"] as Type;
            if (type == null) throw new InvalidDataException($"Supplied item does not contain a valid 'NodeType' definition");
            return type;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Effects = DragDropEffects.All;
            var nodeType = GetListViewType((ListViewItem) dragInfo.SourceItem);
            if (!(Activator.CreateInstance(nodeType) is INode newNode))
            {
                throw new InvalidCastException($"Instantiated class '{nodeType}' does not implement INode");
            }

            dragInfo.Data = newNode;
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return true;
        }

        public void Dropped(IDropInfo dropInfo)
        {
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
        }

        public void DragCancelled()
        {
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return false;
        }
    }
}
