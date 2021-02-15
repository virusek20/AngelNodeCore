using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using AngelNode.Model.Resource;
using GalaSoft.MvvmLight;

namespace AngelNode.Model
{
    public class Character : ObservableObject
    {
        private string _name;
        private Pose _defaultPose;
        private Pose _showcasePose;
        private int _height;
        private float _pitch = 1f;
        private Color _color = Color.FromArgb(255, 149, 228, 246); // Main AW color
        private string _phoneNumber;
        private File _phonePicture;

        public string Name
        {
            get => _name;
            set { Set(() => Name, ref _name, value); }
        }

        public Pose DefaultPose
        {
            get => _defaultPose;
            set { Set(() => DefaultPose, ref _defaultPose, value); }
        }

        /// <summary>
        /// Gets or sets the pose use in character outfit switcher
        /// </summary>
        public Pose ShowcasePose
        {
            get => _showcasePose;
            set { Set(() => ShowcasePose, ref _showcasePose, value); }
        }

        /// <summary>
        /// Gets or sets the height offset of the character (-30 to 30)
        /// </summary>
        public int Height
        {
            get => _height;
            set { Set(() => Height, ref _height, value); }
        }

        /// <summary>
        /// Gets or sets the pitch of the text scroll sound for this character
        /// </summary>
        public float Pitch
        {
            get => _pitch;
            set { Set(() => Pitch, ref _pitch, value); }
        }

        public Color Color
        {
            get => _color;
            set { Set(() => Color, ref _color, value); }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => Set(() => PhoneNumber, ref _phoneNumber, value);
        }

        public File PhonePicture
        {
            get => _phonePicture;
            set => Set(() => PhonePicture, ref _phonePicture, value);
        }

        /// <summary>
        /// Gets the observable collection containing shared poses
        /// </summary>
        public ObservableCollection<Pose> SharedPoses { get; } = new ObservableCollection<Pose>();

        /// <summary>
        /// Gets the observable collection containing outfit poses
        /// </summary>
        public ObservableCollection<Pose> OutfitPoses { get; } = new ObservableCollection<Pose>();
        /// <summary>
        /// Gets the observable collection containing all poses for this character
        /// </summary>
        public ObservableCollection<Pose> Poses => new ObservableCollection<Pose>(SharedPoses.Union(OutfitPoses));
        public ObservableCollection<Outfit> Outfits { get; } = new ObservableCollection<Outfit>();

        /// <summary>
        /// Removes all current outfit poses and creates new one according to the 1st outfit
        /// </summary>
        public void RebuildOutfitPoses()
        {
            OutfitPoses.Clear();
            var outfit = Outfits.FirstOrDefault();
            if (outfit == null) return;

            foreach (var resource in outfit.Directory.Files)
            {
                if (!(resource is File file) || file.GuessType() != ResourceType.Image) continue;

                OutfitPoses.Add(new Pose
                {
                    File = file,
                    Name = System.IO.Path.GetFileNameWithoutExtension(file.Path),
                    Relative = true
                });
            }
        }
    }
}
