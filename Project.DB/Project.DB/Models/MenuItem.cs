using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight;

namespace Design.Models
{
    public class MenuItem : ViewModelBase
    {
        public string Title { get; set; }
        public Symbol SymbolIcon { get; set; }
        public Type NavigateTo { get; set; }
    }
}