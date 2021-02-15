using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AngelNode.Model.Node;

namespace AngelNode.Util.Node
{
    public class NodeTemplateSelector : DataTemplateSelector
    {
        public ResourceDictionary DialogueTemplates { get; set; }
        public DataTemplate EmptyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null || !(item is INode node)) return EmptyTemplate;

            var template = DialogueTemplates.Values.OfType<DataTemplate>().FirstOrDefault(dt =>
            {
                var type = (Type) dt.DataType;
                return type.IsEquivalentTo(node.GetType());
            });

            return template ?? base.SelectTemplate(item, container);
        }
    }
}
