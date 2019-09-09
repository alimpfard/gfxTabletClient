using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gfxTabletClient
{
    /// <summary>
    /// Interaction logic for CapabilityViewWindow.xaml
    /// </summary>
    public partial class CapabilityViewWindow : Window
    {
        public CapabilityViewWindow()
        {
            InitializeComponent();
        }

        private void MTxtClickHere_StylusDown(object sender, StylusDownEventArgs e)
        {
            var point = e.GetStylusPoints((IInputElement)sender).First();
            List<string> available_properties = new List<string>();
            foreach (var prop in point.Description.GetStylusPointProperties())
                available_properties.Add($"{prop.ToString()} => {point.GetPropertyValue(prop)}");
            mListView.ItemsSource = available_properties;
        }
    }
}
