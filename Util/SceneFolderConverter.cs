using AngelNode.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;

namespace AngelNode.Util
{
    class SceneFolderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is IList<Scene> scenes)) throw new InvalidDataException();

            return scenes
                .GroupBy(s => s.Tag.Trim())
                .Select(g => {
                    var folder = new SceneFolder
                    {
                        Name = g.Key
                    };

                    if (string.IsNullOrWhiteSpace(folder.Name)) folder.Name = "Untagged";
                    foreach (var scene in g) folder.Scenes.Add(scene);
                    return folder;
                })
                .OrderBy(f => f.Name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
