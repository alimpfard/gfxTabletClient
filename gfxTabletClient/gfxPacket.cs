using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace gfxTabletClient
{
    [StructLayoutAttribute(LayoutKind.Explicit)]
    public unsafe struct gfxPacket
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9), FieldOffset(0)]
        public byte[] signature;

        [FieldOffset(9)]
        public UInt16 vnum;

        [FieldOffset(11)]
        public byte eventType;

        [FieldOffset(12)]
        public UInt16 x;

        [FieldOffset(14)]
        public UInt16 y;

        [FieldOffset(16)]
        public UInt16 pressure;

        [FieldOffset(18)]
        public Int16 button; // -1 : stylus in range, -2 : tap/click/button 0, -3 : extra button 1, -4 : extra button 2, 

        [FieldOffset(20)]
        public byte down;  // 1 = down, 0 = up


        public override string ToString()
        {
            return $"GfxPacket {{ version {vnum} | {x},{y} - {pressure} | {button} -> {down} }}";
        }
    }
}
