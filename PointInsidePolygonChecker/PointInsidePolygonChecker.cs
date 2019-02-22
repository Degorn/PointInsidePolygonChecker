using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace PointInsidePolygonChecker
{
    public class PointInsidePolygonChecker
    {
        public bool IsInside(Point point, PathFigure polygon)
        {
            var segments = polygon.Segments;
            Point zero = new Point(0, 0);

            List<Point> intersections = new List<Point>();
            for (int i = 0; i < segments.Count; i++)
            {
                List<Point> newIntersections = GetIntersections(zero, point, segments[i]);
                for (int j = 0; j < newIntersections.Count; j++)
                {
                    bool seen = false;
                    Point intersection = newIntersections[j];
                    for (int k = 0; k < intersections.Count; k++)
                    {
                        if (CoordEqual(intersections[k], intersection))
                        {
                            seen = true;
                            break;
                        }
                    }

                    if (!seen)
                    {
                        intersections.Add(intersection);
                    }
                }
            }

            return intersections.Count % 2 == 1;
        }

        private bool CoordEqual(Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        private List<Point> GetIntersections(Point zero, Point point, PathSegment pathSegment)
        {
            List<Point> coords = GetPathSegmentCoords(pathSegment);
            if (pathSegment is QuadraticBezierSegment ||
                pathSegment is BezierSegment)
            {
                return IntersectBezierLine(coords[0], coords[1], coords[2], coords[3], zero, point);
            }
            else if (pathSegment is LineSegment)
            {
                return IntersectLineLine(coords[0], coords[1], zero, point);
            }
        }

        private List<Point> GetPathSegmentCoords(PathSegment pathSegment)
        {
            List<Point> points = new List<Point>();
            if (pathSegment is LineSegment lineSegment)
            {
                points.Add(lineSegment.Point);
            }
            else if (pathSegment is BezierSegment bezierSegment)
            {
                points.Add(bezierSegment.Point1);
                points.Add(bezierSegment.Point2);
                points.Add(bezierSegment.Point3);
            }
            return points;
        }
    }
}
