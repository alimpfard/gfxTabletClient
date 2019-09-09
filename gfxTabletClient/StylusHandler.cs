using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace gfxTabletClient
{
    struct StylusHandler
    {
        private StylusDevice mStylusDevice;
        private int mPressureCap;
        private IInputElement rel;
        public StylusHandler(int pressureCap, IInputElement el)
        {
            mStylusDevice = Stylus.CurrentStylusDevice;
            mPressureCap = pressureCap;
            rel = el;
            if (mStylusDevice == null)
                throw new Exception("No stylus device");
        }

        public StylusPointCollection Points
        {
            get => mStylusDevice.GetStylusPoints(rel);
        }
    }
}
