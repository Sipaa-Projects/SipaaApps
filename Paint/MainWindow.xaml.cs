using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Paint
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private Point startPoint;
        private Path currentPath;
        private SolidColorBrush currentColor;

        public MainWindow()
        {
            this.InitializeComponent();
            currentColor = new SolidColorBrush(Microsoft.UI.Colors.Black);
        }

        private void Canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            startPoint = e.GetCurrentPoint(canvas).Position;
            currentPath = new Path
            {
                Data = new PathGeometry
                {
                    Figures = new PathFigureCollection
                    {
                        new PathFigure
                        {
                            StartPoint = startPoint,
                            IsClosed = false,
                            IsFilled = false,
                            Segments = new PathSegmentCollection
                            {
                                new PolyLineSegment
                                {
                                    Points = new PointCollection { startPoint },
                                },
                            },
                        },
                    },
                },
                Stroke = currentColor,
                StrokeThickness = 2,
            };
            canvas.Children.Add(currentPath);
        }

        private void Canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (currentPath != null)
            {
                Point currentPoint = e.GetCurrentPoint(canvas).Position;
                ((currentPath.Data as PathGeometry).Figures[0].Segments[0] as PolyLineSegment).Points.Add(currentPoint);
            }
        }

        private void Canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            currentPath = null;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            var c = ((Button)sender).Background as SolidColorBrush;
            currentColor = c;
        }
    }
}
