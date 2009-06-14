using System.Windows.Media;
using System.Windows.Shapes;

namespace FarseerGames.WaterSampleSilverlight
{
    /// <summary>
    /// Consolidates creation of shapes.
    /// </summary>
    public class ShapeFactory
    {
        public static Polygon CreatePolygon(PointCollection points, Color? fillColor, Color? borderColor,
                                            double? borderWidth)
        {
            Polygon polygon = new Polygon();
            polygon.Points = points;
            if (borderColor != null)
            {
                polygon.Stroke = new SolidColorBrush((Color) borderColor);
            }

            if (borderWidth != null)
            {
                polygon.StrokeThickness = (double) borderWidth;
            }

            if (fillColor != null)
            {
                polygon.Fill = new SolidColorBrush((Color) fillColor);
            }
            return polygon;
        }

        public static Rectangle CreateRectangle(float width, float height, Color fill, Color stroke,
                                                float strokeThickness)
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = width;
            rectangle.Height = height;
            rectangle.Fill = new SolidColorBrush(fill);
            rectangle.Stroke = new SolidColorBrush(stroke);
            rectangle.StrokeThickness = strokeThickness;
            return rectangle;
        }
    }
}