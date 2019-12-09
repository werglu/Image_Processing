using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltrowanieObrazu
{
    class MultiPolygon
    {
        public List<Polygon> multiPolygons = new List<Polygon>();
        public (int, int) controlEdge = (-1, -1);

        public MultiPolygon()
        {
            multiPolygons.Add(new Polygon());
        }

        public (int, int) FindEdge(Point clickPoint, bool changeColor = true)
        {
            float x = clickPoint.X;
            float y = clickPoint.Y;
            int p = 0, v = 0;
            foreach (var polygon in multiPolygons)
            {
                v = 0;
                foreach (var vertex in polygon.vertices)
                {
                    if (v < polygon.vertices.Count)
                    {   // z v wychodzi interesujaca nas krawedz
                        double x1 = vertex.edges.start.X;
                        double y1 = vertex.edges.start.Y;
                        double x2 = vertex.edges.end.X;
                        double y2 = vertex.edges.end.Y;
                        double d = Math.Abs((double)(y2 - y1) / (double)(x2 - x1) * (double)x - (double)y + (double)(x2 * y1 - x1 * y2) / (double)(x2 - x1)) / Math.Sqrt((double)(y2 - y1) / (double)(x2 - x1) * (double)(y2 - y1) / (double)(x2 - x1) + 1);
                        if (d < 4)
                        {
                            if (y2 > y1)
                            {
                                if (y >= (y1 - 1) && y <= (y2 + 1))
                                {
                                    if (x2 > x1)
                                    {
                                        if (x >= (x1 - 1) && x <= (x2 + 1))
                                        {
                                            controlEdge = (p, v);
                                            return (p, v);
                                        }

                                    }
                                    else
                                    {
                                        if (x >= (x2 - 1) && x <= (x1 + 1))
                                        {
                                            controlEdge = (p, v);
                                            return (p, v);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (y >= (y2 - 1) && y <= (y1 + 1))
                                {
                                    if (x2 > x1)
                                    {
                                        if (x >= (x1 - 1) && x <= (x2 + 1))
                                        {
                                            controlEdge = (p, v);
                                            return (p, v);
                                        }

                                    }
                                    else
                                    {
                                        if (x >= (x2 - 1) && x <= (x1 + 1))
                                        {
                                            controlEdge = (p, v);
                                            return (p, v);
                                        }
                                    }
                                }
                            }

                        }
                    }
                    v++;
                }
                p++;
            }
            controlEdge = (-1, -1);

            return (-1, -1);
        }

        public (int, int) FindVertexInPolygons(Point clickPoint)
        {
            int p = 0, v = 0;
            foreach (var polygon in multiPolygons)
            {
                v = 0;
                foreach (var vertex in polygon.vertices)
                {
                    if (clickPoint.X <= vertex.middle.X + 2.5 && clickPoint.X >= vertex.middle.X - 2.5 && clickPoint.Y <= vertex.middle.Y + 2.5 && clickPoint.Y >= vertex.middle.Y - 2.5)
                    {
                        return (p, v);
                    }
                    v++;
                }
                p++;
            }
            return (-1, -1);
        }
    }
}
