using System.IO;
using AngelNode.Model.Resource;
using GalaSoft.MvvmLight;
using File = AngelNode.Model.Resource.File;

namespace AngelNode.Model.Node
{
    public class NodeChangeBackground : ObservableObject, INode
    {
        public enum BackgroundTransitionTypeEnum
        {
            NewDay,
            Blend,
            Instant,
            FadeToBlack
        }

        public enum BackgroundTransitionSpeedEnum
        {
            Slow,
            Medium,
            Fast
        }

        private File _background;
        private BackgroundTransitionTypeEnum _transitionType = BackgroundTransitionTypeEnum.Blend;
        private BackgroundTransitionSpeedEnum _transitionSpeed = BackgroundTransitionSpeedEnum.Fast;

        public File Background
        {
            get => _background;
            set
            {
                if (value != null && value.GuessType() != ResourceType.Image) throw new InvalidDataException();

                Set(() => Background, ref _background, value);
            }
        }

        public BackgroundTransitionTypeEnum TransitionType
        {
            get => _transitionType;
            set { Set(() => TransitionType, ref _transitionType, value); }
        }

        public BackgroundTransitionSpeedEnum TransitionSpeed
        {
            get => _transitionSpeed;
            set { Set(() => TransitionSpeed, ref _transitionSpeed, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeChangeBackground(this);
        }
    }
}
