using AngelNode.Model;
using GalaSoft.MvvmLight;

namespace AngelNode.ViewModel
{
    public class VariableViewModel : ViewModelBase
    {
        private Variable _variable;

        public Variable Variable
        {
            get => _variable;
            set { Set(() => Variable, ref _variable, value); }
        }
    }
}
