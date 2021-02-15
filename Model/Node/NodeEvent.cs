using GalaSoft.MvvmLight;

namespace AngelNode.Model.Node
{
    public class NodeEvent : ObservableObject, INode
    {
        public enum EventTypeEnum
        {
            HookBrackets,
            Call,
            CallEnd,
            Contacts,
            Custom
        }

        private EventTypeEnum _eventType;

        private string _eventName;
        private Character _caller;
        private string _text;
        private string _phoneTime;
        private float _duration = 3f;
        private bool _blackOnWhite = false;
        private bool _isPlayerInitiated = false;
        private bool _isOngoing = false;

        public string EventName
        {
            get => _eventName;
            set { Set(() => EventName, ref _eventName, value); }
        }

        public Character Caller
        {
            get => _caller;
            set { Set(() => Caller, ref _caller, value); }
        }
        public string PhoneTime
        {
            get => _phoneTime;
            set { Set(() => PhoneTime, ref _phoneTime, value); }
        }

        public string Text
        {
            get => _text;
            set { Set(() => Text, ref _text, value); }
        }

        public EventTypeEnum EventType
        {
            get => _eventType;
            set { Set(() => EventType, ref _eventType, value); }
        }

        public float Duration
        {
            get => _duration;
            set { Set(() => Duration, ref _duration, value); }
        }

        public bool BlackOnWhite
        {
            get => _blackOnWhite;
            set => Set(() => BlackOnWhite, ref _blackOnWhite, value);
        }

        public bool IsPlayerInitiated
        {
            get => _isPlayerInitiated;
            set => Set(() => IsPlayerInitiated, ref _isPlayerInitiated, value);
        }

        public bool IsOngoing 
        {
            get => _isOngoing;
            set => Set(() => IsOngoing, ref _isOngoing, value);
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodeEvent(this);
        }
    }
}
