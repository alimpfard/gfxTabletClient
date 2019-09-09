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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gfxTabletClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StylusHandler? handler = null;
        IDeviceUpdater mDeviceUpdater;
        string mIP = "192.168.1.2", mPort = "40118";
        double xmax, ymax;
        public MainWindow()
        {
            InitializeComponent();
            xmax = textbox.Width;
            ymax = textbox.Height;
            IniFile iniFile = new IniFile();
            if (!iniFile.KeyExists("ServerIP"))
            {
                iniFile.Write("ServerIP", mIP);
            }
            else
            {
                mIP = iniFile.Read("ServerIP");
            }
            if (!iniFile.KeyExists("ServerPort"))
            {
                iniFile.Write("ServerPort", mPort);
            }
            else
            {
                mPort = iniFile.Read("ServerPort");
            }
            mDeviceUpdater = new UdpSenderDeviceUpdater(mIP, mPort);
        }
        private void MCanvas_StylusMove(object sender, StylusEventArgs e)
        {
            float pressure = e.GetStylusPoints((IInputElement)sender).Select(i => i.PressureFactor).Average();
            var xy = e.GetPosition((IInputElement)sender);
            var x = xy.X;
            var y = xy.Y;
            bool inair = e.InAir;
            var point = e.GetStylusPoints((IInputElement)sender).First();

            // point descriptors
            bool barrel = default;
            int pitch = default;
            int roll = default;
            int yaw = default;

            if (point.Description.HasProperty(StylusPointProperties.BarrelButton))
                barrel = point.GetPropertyValue(StylusPointProperties.BarrelButton) != 0;

            /* if (point.Description.HasProperty(StylusPointProperties.PitchRotation))
                pitch = point.GetPropertyValue(StylusPointProperties.PitchRotation);

            if (point.Description.HasProperty(StylusPointProperties.RollRotation))
                roll = point.GetPropertyValue(StylusPointProperties.RollRotation);

            if (point.Description.HasProperty(StylusPointProperties.YawRotation))
                yaw = point.GetPropertyValue(StylusPointProperties.YawRotation); */

            mDeviceUpdater.ProcessAndEnqueueUpdate(x, y, xmax, ymax, pressure, inair, false, barrel);
            textbox.Text = $"at {x}, {y} with pressure {pressure} {(inair ? "in air" : "touching")} - {(barrel ? "barrel" : "")} - {yaw};{pitch};{roll}"; 
        }

        private void Textbox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            xmax = e.NewSize.Width;
            ymax = e.NewSize.Height;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mDeviceUpdater.Stop();
        }

        private void MCanvas_StylusDown(object sender, StylusDownEventArgs e)
        {
            Stylus.Capture((IInputElement)sender);
        }

        private void MCanvas_StylusUp(object sender, StylusEventArgs e)
        {
            Stylus.Capture((IInputElement)sender, CaptureMode.None);
        }
    }
}
