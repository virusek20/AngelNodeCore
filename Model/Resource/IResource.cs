using System.ComponentModel;

namespace AngelNode.Model.Resource
{
    public enum ResourceType
    {
        Image,
        Sound,
        Unknown
    }

    public interface IResource : INotifyPropertyChanged
    {
        string Path { get; set; }
    }
}
