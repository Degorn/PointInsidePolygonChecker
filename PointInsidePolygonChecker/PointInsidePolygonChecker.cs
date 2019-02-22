﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace PointInsidePolygonChecker
{
    public class PointInsidePolygonChecker
    {
        private readonly double tolerance = 1E-6;

        public bool IsInside(Point point, PathFigure polygon)
        {
            Point segmentStartPoint = polygon.StartPoint;
            PathSegmentCollection segments = polygon.Segments;
            Point zero = new Point(0, 0);

            List<Point> intersections = new List<Point>();
            for (int i = 0; i < segments.Count; i++)
            {
                List<Point> newIntersections = GetIntersections(zero, point, segmentStartPoint, segments[i]);
                segmentStartPoint = GetPathSegmentCoords(segments[i]).Last();
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

        private Point CoordMin(Point a1, Point a2)
        {
            return new Point(Math.Min(a1.X, a2.X), Math.Min(a1.Y, a2.Y));
        }

        private Point CoordMax(Point a1, Point a2)
        {
            return new Point(Math.Max(a1.X, a2.X), Math.Max(a1.Y, a2.Y));
        }

        private Point CoordMultiply(Point c, double f)
        {
            return new Point(c.X * f, c.Y * f);
        }

        private Point CoordAdd(Point c1, Point c2)
        {
            return new Point(c1.X + c2.X, c1.Y + c2.Y);
        }

        private double CoordDot(Point c1, Point c2)
        {
            return c1.X * c2.X + c1.Y * c2.Y;
        }

        private List<double> LinearRoot(double p2, double p1)
        {
            List<double> results = new List<double>();

            var a = p2;
            if (a != 0)
            {
                results.Add(-p1 / p2);
            }

            return results;
        }

        private List<double> QuadRoots(double p3, double p2, double p1)
        {
            List<double> results = new List<double>();

            if (Math.Abs(p3) <= tolerance)
            {
                return LinearRoot(p2, p1);
            }

            var a = p3;
            var b = p2 / a;
            var c = p1 / a;
            var d = b * b - 4 * c;
            if (d > 0)
            {
                var e = Math.Sqrt(d);
                results.Add(0.5 * (-b + e));
                results.Add(0.5 * (-b - e));
            }
            else if (d == 0)
            {
                results.Add(0.5 * -b);
            }

            return results;
        }

        private List<double> CubeRoots(double p4, double p3, double p2, double p1)
        {
            if (Math.Abs(p4) <= tolerance)
            {
                return QuadRoots(p3, p2, p1);
            }

            List<double> results = new List<double>();
            var c3 = p4;
            var c2 = p3 / c3;
            var c1 = p2 / c3;
            var c0 = p1 / c3;

            var a = (3 * c1 - c2 * c2) / 3;
            var b = (2 * c2 * c2 * c2 - 9 * c1 * c2 + 27 * c0) / 27;
            var offset = c2 / 3;
            var discrim = b * b / 4 + a * a * a / 27;
            var halfB = b / 2;

            double tmp;
            if (discrim > 0)
            {
                var e = Math.Sqrt(discrim);
                tmp = -halfB + e;
                var root = tmp >= 0 ? Math.Pow(tmp, 1 / 3) : -Math.Pow(-tmp, 1 / 3);
                tmp = -halfB - e;
                if (tmp >= 0)
                {
                    root += Math.Pow(tmp, 1 / 3);
                }
                else
                {
                    root -= Math.Pow(-tmp, 1 / 3);
                }
                results.Add(root - offset);
            }
            else if (discrim < 0)
            {
                var distance = Math.Sqrt(-a / 3);
                var angle = Math.Atan2(Math.Sqrt(-discrim), -halfB) / 3;
                var cos = Math.Cos(angle);
                var sin = Math.Sign(angle);
                var sqrt3 = Math.Sqrt(3);
                results.Add(2 * distance * cos - offset);
                results.Add(-distance * (cos + sqrt3 * sin) - offset);
                results.Add(-distance * (cos - sqrt3 * sin) - offset);
            }
            else
            {
                if (halfB >= 0)
                {
                    tmp = -Math.Pow(halfB, 1 / 3);
                }
                else
                {
                    tmp = Math.Pow(-halfB, 1 / 3);
                }
                results.Add(2 * tmp - offset);
                results.Add(-tmp - offset);
            }

            return results;
        }

        private Point CoordLerp(Point c1, Point c2, double t)
        {
            return new Point(c1.X + (c2.X - c1.X) * t, c1.Y + (c2.Y - c1.Y) * t);
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
            else
            {
                throw new Exception($"Unsupported segment type: {pathSegment.GetType()}");
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

        private List<Point> IntersectBezierLine(Point p1, Point p2, Point p3, Point p4, Point a1, Point a2)
        {
            List<Point> result = new List<Point>();
            Point min = CoordMin(a1, a2);
            Point max = CoordMax(a1, a2);

            var a = CoordMultiply(p1, -1);
            var b = CoordMultiply(p2, 3);
            var c = CoordMultiply(p3, -3);
            var c3 = CoordAdd(a, CoordAdd(b, CoordAdd(c, p4)));

            a = CoordMultiply(p1, 3);
            b = CoordMultiply(p2, -6);
            c = CoordMultiply(p3, 3);
            var c2 = CoordAdd(a, CoordAdd(b, c));

            a = CoordMultiply(p1, -3);
            b = CoordMultiply(p2, 3);
            var c1 = CoordAdd(a, b);

            var c0 = p1;

            var n = new Point(a1.Y - a2.Y, a2.X - a1.X);
            var cl = a1.X * a2.Y - a2.X * a1.Y;

            var roots = CubeRoots(CoordDot(n, c3),
                                  CoordDot(n, c2),
                                  CoordDot(n, c1),
                                  CoordDot(n, c0) + cl);

            for (int i = 0; i < roots.Count; i++)
            {
                var t = roots[i];

                if (t >= 0 && t <= 1)
                {
                    var p5 = CoordLerp(p1, p2, t);
                    var p6 = CoordLerp(p2, p3, t);
                    var p7 = CoordLerp(p3, p4, t);

                    var p8 = CoordLerp(p5, p6, t);
                    var p9 = CoordLerp(p6, p7, t);

                    var p10 = CoordLerp(p8, p9, t);

                    if (a1.X == a2.X)
                    {
                        if (min.Y <= p10.Y && p10.Y <= max.Y)
                        {
                            result.Add(p10);
                        }
                    }
                    else if (a1.Y == a2.Y)
                    {
                        if (min.X <= p10.X && p10.X <= max.X)
                        {
                            result.Add(p10);
                        }
                    }
                    else if (min.X <= p10.X && p10.X <= max.X && min.Y <= p10.Y && p10.Y <= max.Y)
                    {
                        result.Add(p10);
                    }
                }
            }

            return result;
        }
    }
}
