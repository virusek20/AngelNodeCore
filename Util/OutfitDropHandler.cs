using AngelNode.Model.Resource;
using AngelNode.ViewModel;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace AngelNode.Util
{
    class OutfitDropHandler : IDropTarget
    {
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is Directory)
            {
                dropInfo.Effects = DragDropEffects.All;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dataContext = (CharacterViewModel)((ListView)dropInfo.VisualTarget).DataContext;
            
            var outfits = dataContext.Character?.Outfits;
            var directory = dropInfo.Data as Directory;

            if (outfits.Any(outfit => outfit.Directory?.Path == directory.Path)) return;

            outfits.Add(new Model.Outfit
            {
                Directory = directory,
                Name = directory.Name
            });

            // First added outfit, populate outfit poses
            if (outfits.Count == 1) dataContext.Character.RebuildOutfitPoses();
        }
    }
}
