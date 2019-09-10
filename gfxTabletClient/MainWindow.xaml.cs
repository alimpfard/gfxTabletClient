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
        IniFile iniFile;
        List<Button> leftShortcutButtons, rightShortcutButtons;

        public MainWindow()
        {
            InitializeComponent();
            xmax = textbox.Width;
            ymax = textbox.Height;
            iniFile = new IniFile();
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
            leftShortcutButtons = new List<Button>();
            rightShortcutButtons = new List<Button>();

            leftShortcutButtons.Add(createButton("+>", Addbutt_left_Click));
            
            rightShortcutButtons.Add(createButton("<+", Addbutt_right_Click));

            rightShortcutButtons.Add(createButton("B1", (sender, e) => { mDeviceUpdater.EmitRawClick(1, true); mDeviceUpdater.EmitRawClick(1, false); }));
            rightShortcutButtons.Add(createButton("B2", (sender, e) => { mDeviceUpdater.EmitRawClick(2, true); mDeviceUpdater.EmitRawClick(2, false); }));
            rightShortcutButtons.Add(createButton("B3", (sender, e) => { mDeviceUpdater.EmitRawClick(3, true); mDeviceUpdater.EmitRawClick(3, false); }));

            mButtonsListLeft.ItemsSource = leftShortcutButtons;
            mButtonsListRight.ItemsSource = rightShortcutButtons;

            mDeviceUpdater = new UdpSenderDeviceUpdater(mIP, mPort);
        }
        
        private Button createButton(object content, RoutedEventHandler callback)
        {
            var button = new Button() { Content = content };
            button.Click += callback;
            return button;
        }

        static Thickness
            left32 = new Thickness(-32, 0, 0, 0),
            zero = new Thickness(0),
            right32 = new Thickness(0, 0, -32, 0),
            leftz32 = new Thickness(32, 0, 0, 0);

        private void Addbutt_right_Click(object sender, RoutedEventArgs e)
        {
            if (mButtonsListLeft.Margin.Left == 0)
            {
                mButtonsListLeft.Margin = left32;
                mCanvas.Margin = zero;
            }
            else
            {
                mButtonsListLeft.Margin = zero;
                mCanvas.Margin = leftz32;
            }
        }

        private void Addbutt_left_Click(object sender, RoutedEventArgs e)
        {
            if (mButtonsListRight.Margin.Right == 0)
                mButtonsListRight.Margin = right32;
            else
                mButtonsListRight.Margin = zero;
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

        private void Textbox_StylusEnter(object sender, StylusEventArgs e)
        {
            mDeviceUpdater.SignalHovering(true);
        }

        private void Textbox_StylusOutOfRange(object sender, StylusEventArgs e)
        {
            mDeviceUpdater.SignalHovering(false);
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Not yet implemented XD");
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            new CapabilityViewWindow().Show();
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
