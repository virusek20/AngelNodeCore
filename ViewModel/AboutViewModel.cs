using System.Reflection;
using GalaSoft.MvvmLight;

namespace AngelNode.ViewModel
{
    public class AboutViewModel : ViewModelBase
    {
        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
