using AngelNode.Model.Node;

namespace AngelNode.Service.Interface
{
    public interface IExportService
    {
        void Export(string path);
        void Run(int startNode);
        int GetNodeNumber(INode node);
        string DefaultExtension { get; }
        string FileFilter { get; }
    }
}
