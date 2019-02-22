using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace PointInsidePolygonChecker
{
    public class PointInsidePolygonChecker
    {
        public bool IsInside(Point point, PathFigure polygon)
        {
            Point segmentStartPoint = polygon.StartPoint;
            PathSegmentCollection segments = polygon.Segments;
            Point zero = new Point(0, 0);

            List<Point> intersections = new List<Point>();
            for (int i = 0; i < segments.Count; i++)
            {
                List<Point> newIntersections = GetIntersections(zero, point, segmentStartPoint, segments[i]);
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

        private List<Point> GetIntersections(Point zero, Point point, Point segmentPreviousPoint, PathSegment pathSegment)
        {
            List<Point> coords = new List<Point>
            {
                segmentPreviousPoint,
            };
            coords.AddRange(GetPathSegmentCoords(pathSegment));
            if (pathSegment is QuadraticBezierSegment ||
                pathSegment is BezierSegment)
            {
                return IntersectBezierLine(coords[0], coords[1], coords[2], coords[3], zero, point);
            }
            else if (pathSegment is LineSegment)
            {
                return new List<Point>
                {
                    IntersectLineLine(coords[0], coords[1], zero, point)
                };
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
            else
            {
                throw new Exception($"Unsupported segment type: {pathSegment.GetType()}");
            }
            return points;
        }

        private Point IntersectLineLine(Point a1, Point a2, Point b1, Point b2)
        {
            var ua_t = (b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X);
            var ub_t = (a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X);
            var u_b =  (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);

            if (u_b != 0)
            {
                var ua = ua_t / u_b;
                var ub = ub_t / u_b;

                if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
                {
                    return new Point(
                            a1.X + ua * (a2.X - a1.X),
                            a1.Y + ua * (a2.Y - a1.Y));
                }
            }

            return new Point();
        }
    }
}
