using System;
using System.Text;
using AngelNode.Model.Node;
using AngelNode.Service.Interface;
using GalaSoft.MvvmLight.Ioc;

namespace AngelNode.Service.Implementations.RenPy
{
    public class RenPyExportService : IExportService
    {
        private readonly IProjectService _projectService;

        public RenPyExportService()
        {
            _projectService = SimpleIoc.Default.GetInstance<IProjectService>();
        }

        public void Export(string path)
        {
            StringBuilder output = new StringBuilder();
            ExportScenes(output);

            System.IO.File.WriteAllText(path, output.ToString());
        }

        public void Run(int startNode)
        {
            throw new NotImplementedException();
        }

        public int GetNodeNumber(INode node)
        {
            throw new NotImplementedException();
        }

        public string DefaultExtension { get; } = ".py";
        public string FileFilter { get; } = "Python file|*.py";

        private void ExportScenes(StringBuilder builder)
        {
            var nodeVisitor = new RenPyNodeVisitor();

            foreach (var scene in _projectService.Scenes)
            {
                foreach (var node in scene.Nodes) node.Accept(nodeVisitor);
            }

            builder.Append(nodeVisitor);
        }
    }
}
