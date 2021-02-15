using System.ComponentModel;

namespace AngelNode.Model.Node
{
    public interface INode : INotifyPropertyChanged
    {
        void Accept(INodeVisitor nodeVisitor);
    }
}
