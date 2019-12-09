using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltrowanieObrazu
{
    class Edges
    {
        public Point start;
        public Point end;
        public double x = -1;
        public Edges(Point start, Point end)
        {
            this.start = start;
            this.end = end;
        }
    }
}
