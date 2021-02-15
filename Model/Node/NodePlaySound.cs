using System.IO;
using AngelNode.Model.Resource;
using GalaSoft.MvvmLight;
using File = AngelNode.Model.Resource.File;

namespace AngelNode.Model.Node
{
    public class NodePlaySound : ObservableObject, INode
    {
        public enum SoundTypeEnum
        {
            Music,
            SFX
        }

        private File _sound;
        private SoundTypeEnum _soundType;
        private float _startTime;
        private float _volume = 1.0f;

        public File Sound
        {
            get => _sound;
            set
            {
                if (value != null && value.GuessType() != ResourceType.Sound) throw new InvalidDataException();

                Set(() => Sound, ref _sound, value);
            }
        }

        public SoundTypeEnum SoundType
        {
            get => _soundType;
            set { Set(() => SoundType, ref _soundType, value); }
        }

        public float StartTime
        {
            get => _startTime;
            set { Set(() => StartTime, ref _startTime, value); }
        }

        public float Volume
        {
            get => _volume;
            set { Set(() => Volume, ref _volume, value); }
        }

        public void Accept(INodeVisitor nodeVisitor)
        {
            nodeVisitor.VisitNodePlaySound(this);
        }
    }
}
