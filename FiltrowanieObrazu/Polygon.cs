using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiltrowanieObrazu
{
    class Polygon
    {
        public List<Vertex> vertices = new List<Vertex>();
        public bool newPolygon = true;
        public bool niepomalowany = true;
        public bool doUsuniecia = false;

        public void AddVertex(Point clickPoint)
        {
            Vertex vertex = new Vertex(clickPoint, Color.Black);
            if (vertices.Count == 0) //first vertex
            {
                Edges edge = new Edges(clickPoint, new Point(-1, -1));
                vertex.edges = edge;
                newPolygon = false;
            }
            else
            {
                Vertex prevVertex = vertices.Last();
                prevVertex.edges.end = clickPoint;
                Edges edge = new Edges(clickPoint, new Point(-1, -1));

                if (clickPoint.X <= vertices.First().middle.X + 4 && clickPoint.X >= vertices.First().middle.X - 4 && clickPoint.Y <= vertices.First().middle.Y + 4 && clickPoint.Y >= vertices.First().middle.Y - 4) //end of polygon
                {
                    edge.end = vertices.First().middle;
                    vertex.edges = edge;
                    newPolygon = true;
                    return;

                }

                vertex.edges = edge;
            }
            vertices.Add(vertex);
        }

    }
}
