using System.Drawing;
using System.Windows.Forms;

namespace Finances
{
    internal class RoundedPanel :Control
    {
        public Color BackColor { get; set; }
        public Size Size { get; set; }
        public Point Location { get; set; }
        public int CornerRadius { get; set; }
    }
}