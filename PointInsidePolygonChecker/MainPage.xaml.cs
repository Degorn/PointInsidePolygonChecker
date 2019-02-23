using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PointInsidePolygonChecker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PointInsidePolygonChecker checker;
        private PathFigure figure;
        private UIElement clickPoint;

        public MainPage()
        {
            this.InitializeComponent();

            InitComponents();
            InitVisuals();
        }

        private void InitComponents()
        {
            checker = new PointInsidePolygonChecker();
            clickPoint = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Colors.Red),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1
            };
        }

        private void InitVisuals()
        {
            figure = new PathFigure()
            {
                Segments = new PathSegmentCollection()
            };
            figure.StartPoint = new Point(100, 100);
            figure.Segments.Add(new BezierSegment() { Point1 = new Point(50, 40), Point2 = new Point(540, 80), Point3 = new Point(600, 100) });
            figure.Segments.Add(new BezierSegment() { Point1 = new Point(550, 150), Point2 = new Point(40, 500), Point3 = new Point(600, 600) });
            figure.Segments.Add(new LineSegment() { Point = new Point(100, 100) });

            Path path = new Path
            {
                Data = new PathGeometry { Figures = new PathFigureCollection { figure }, FillRule = FillRule.EvenOdd },
                Stroke = new SolidColorBrush(Colors.Black),
            };

            Canv.Children.Add(path);
            Canv.Children.Add(clickPoint);
        }

        private void Canv_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Point pos = e.GetCurrentPoint(Canv).Position;

            Canvas.SetLeft(clickPoint, pos.X);
            Canvas.SetTop(clickPoint, pos.Y);

            var result = checker.IsInside(pos, figure);
            Result.Text = $"Position {pos.X:0}, {pos.Y:0} \nis inside: {result}";
        }
    }
}
