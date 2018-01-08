using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;

namespace ERDTransport
{
    [Serializable]
    class ClientCommand
    {
        public bool needFrame = false;
        public int needWidth = 0;
        public int needHeight = 0;
        public Keys key = Keys.Enter;
        public bool leftMouseClick = false;
        public bool rightMouseClick = false;
        public bool moveCursor = false;
        public float mouseRelativeX = 0;
        public float mouseRelativeY = 0;
    }
}
