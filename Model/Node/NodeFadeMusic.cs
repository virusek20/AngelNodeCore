using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeFadeMusic : ObservableObject, INode
    {
        public enum AudioFadeTypeEnum
        {
            Music,
            SFX,
            All
        }

        private float _fadeTime = 1f;
        private AudioFadeTypeEnum _audioFadeType = AudioFadeTypeEnum.All;

        public float FadeTime
        {
            get => _fadeTime;
            set { Set(() => FadeTime, ref _fadeTime, value); }
        }

        public AudioFadeTypeEnum AudioFadeType
        {
            get => _audioFadeType;
            set { Set(() => AudioFadeType, ref _audioFadeType, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeFadeMusic(this);
        }
    }
}
