using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeShake : ObservableObject, INode
    {
        private bool _shakeBackground;
        private bool _shakeCharacters;
        private float _amplitude = 32f;
        private float _duration = 2f;

        public bool ShakeBackground
        {
            get => _shakeBackground;
            set { Set(() => ShakeBackground, ref _shakeBackground, value); }
        }

        public bool ShakeCharacters
        {
            get => _shakeCharacters;
            set { Set(() => ShakeCharacters, ref _shakeCharacters, value); }
        }

        public float Amplitude
        {
            get => _amplitude;
            set { Set(() => Amplitude, ref _amplitude, value); }
        }

        public float Duration
        {
            get => _duration;
            set { Set(() => Duration, ref _duration, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeShake(this);
        }
    }
}
