using GalaSoft.MvvmLight;
using System.Linq;

namespace AngelNode.Model.Node
{
    public class NodeOutfitUnlocked : ObservableObject, INode
    {
        private Character _character;
        private Outfit _outfit;

        public Character Character
        {
            get => _character;
            set
            {
                Set(() => Character, ref _character, value);
                Set(() => Outfit, ref _outfit, value?.Outfits.FirstOrDefault());
            }
        }

        public Outfit Outfit
        {
            get => _outfit;
            set
            {
                if (Character == null) return;
                if (Character.Outfits.Contains(value)) Set(() => Outfit, ref _outfit, value);
            }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeOutfitUnlocked(this);
        }
    }
}
