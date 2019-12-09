using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltrowanieObrazu
{
    class Vertex
    {
        public Point middle;
        public Color color;
        public Edges edges;
        public Vertex(Point middlePoint, Color color)
        {
            middle = middlePoint;
            this.color = color;
        }
    }
}
