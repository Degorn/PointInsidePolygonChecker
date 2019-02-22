using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PointInsidePolygonChecker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            PathFigure figure = new PathFigure()
            {
                Segments = new PathSegmentCollection()
            };
            figure.StartPoint = new Point(100, 100);
            figure.Segments.Add(new LineSegment() { Point = new Point(600, 100) });
            figure.Segments.Add(new LineSegment() { Point = new Point(600, 600) });
            figure.Segments.Add(new LineSegment() { Point = new Point(100, 600) });

            var figures = new PathFigureCollection
            {
                figure
            };
            Path path = new Path
            {
                Data = new PathGeometry { Figures = figures, FillRule = FillRule.Nonzero },
                Stroke = new SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Colors.Green),
            };
            Canv.Children.Add(path);

            PointInsidePolygonChecker checker = new PointInsidePolygonChecker();
            var result = checker.IsInside(new Point(200, 200), figure);
            Result.Text = result.ToString();
        }
    }
}
