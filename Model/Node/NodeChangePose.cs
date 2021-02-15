using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeChangePose : ObservableObject, INode
    {
        private Character _character;
        private Pose _pose;

        public Character Character
        {
            get => _character;
            set
            {
                Set(() => Character, ref _character, value);
                Set(() => Pose, ref _pose, value?.DefaultPose);
            }
        }

        public Pose Pose
        {
            get => _pose;
            set
            {
                if (Character == null) return;
                if (Character.Poses.Contains(value)) Set(() => Pose, ref _pose, value);
            }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeChangePose(this);
        }
    }
}
