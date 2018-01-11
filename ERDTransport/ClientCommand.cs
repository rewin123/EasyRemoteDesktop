using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using WindowsInput.Native;

namespace ERDTransport
{
    [Serializable]
    class ClientCommand
    {
        public bool needFrame = false;
        public int needWidth = 0;
        public int needHeight = 0;
        public byte pressKey = 0; //0 - noKey, 1 - pressDown, 2 - pressUp
        public VirtualKeyCode key = VirtualKeyCode.ACCEPT;
        public int mouseEvent = 0;
        public bool moveCursor = false;
        public float mouseRelativeX = 0;
        public float mouseRelativeY = 0;
    }
}
