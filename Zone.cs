using System.Drawing;
using System.Drawing.Text;
using Clipper2Lib;

public class Zone
{
    public required string Code { get; init; }

    public PathsD Polygon { get; private set; } = new PathsD();

    public double MinX { get; private set; } = double.MaxValue;
    public double MinY { get; private set; } = double.MaxValue;
    public double MaxX { get; private set; } = double.MinValue;
    public double MaxY { get; private set; } = double.MinValue;

    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;

    public List<Cuboid> Cuboids = new();

    public void AddCuboid(Cuboid cuboid)
    {
        var x1 = Math.Min(cuboid.X1, cuboid.X2);
        var y1 = Math.Min(cuboid.Y1, cuboid.Y2);
        var x2 = Math.Max(cuboid.X1, cuboid.X2);
        var y2 = Math.Max(cuboid.Y1, cuboid.Y2);

        MinX = Math.Min(MinX, x1);
        MinY = Math.Min(MinY, y1);
        MaxX = Math.Max(MaxX, x2);
        MaxY = Math.Max(MaxY, y2);

        var cuboidPath = new PathD();
        cuboidPath.Add(new(x1, y1));
        cuboidPath.Add(new(x1, y2));
        cuboidPath.Add(new(x2, y2));
        cuboidPath.Add(new(x2, y1));

        var cuboidPaths = new PathsD([cuboidPath]);

        Polygon = Clipper.Union(Polygon, cuboidPaths, FillRule.NonZero, 4);

        Cuboids.Add(cuboid);
    }

    public void SubtractCuboid(Cuboid cuboid)
    {
        var x1 = Math.Min(cuboid.X1, cuboid.X2);
        var y1 = Math.Min(cuboid.Y1, cuboid.Y2);
        var x2 = Math.Max(cuboid.X1, cuboid.X2);
        var y2 = Math.Max(cuboid.Y1, cuboid.Y2);

        var cuboidPath = new PathD
        {
            new(x1, y1),
            new(x2, y1),
            new(x2, y2),
            new(x1, y2)
        };

        var cuboidPaths = new PathsD([cuboidPath]);

        Polygon = Clipper.Difference(Polygon, cuboidPaths, FillRule.NonZero, 4);
    }

    public void SubtractZone(Zone zone)
        => Polygon = Clipper.Difference(Polygon, zone.Polygon, FillRule.NonZero, 4);

    public Bitmap DrawLines()
    {
        var btm = new Bitmap((int)Width + 60, (int)Height + 60);
        using var g = Graphics.FromImage(btm);
        g.Clear(Color.White);

        for (int i = 0; i < Polygon.Count; i++)
        {
            var path = Polygon[i];

            for (int j = 0; j < path.Count; j++)
            {
                var point1 = path[j];
                var point2 = path[(j + 1) % path.Count];

                var x1 = point1.x - MinX + 30;
                var y1 = point1.y - MinY + 30;
                var x2 = point2.x - MinX + 30;
                var y2 = point2.y - MinY + 30;

                g.DrawRectangle(Pens.Red, (int)x2 - 1, (int)y2 - 1, 2, 2);

                var vecX = x2 - x1;
                var vecY = y2 - y1;
                var vecLen = Math.Sqrt(vecX * vecX + vecY * vecY);
                if (6 < vecLen)
                {
                    vecX /= vecLen;
                    vecY /= vecLen;

                    x1 += vecX * 3;
                    y1 += vecY * 3;

                    vecX *= vecLen - 6;
                    vecY *= vecLen - 6;

                    x2 = x1 + vecX;
                    y2 = y1 + vecY;
                }

                g.DrawLine(Pens.Black, (int)x1, (int)y1, (int)x2, (int)y2);
                g.DrawRectangle(Pens.Black, (int)x2 - 1, (int)y2 - 1, 2, 2);
            }
        }

        using var fontsCollection = new InstalledFontCollection();
        var fontFamily = fontsCollection.Families.FirstOrDefault(x => x.Name == "Cascadia Code") ?? fontsCollection.Families.FirstOrDefault();
        if (fontFamily is not null)
        {
            var font = new Font(fontFamily, 18f, FontStyle.Regular, GraphicsUnit.Pixel);
            var text = $"There are {Polygon.Count} pathes and {Polygon.SelectMany(x => x).Count()} points; MinX: {MinX:0.##}; MinY: {MinY:0.##}; MaxX: {MaxX:0.##}; MaxY: {MaxY:0.##}";
            g.DrawString(text, font, Brushes.Black, 4, 4);

            // var smallFont = new Font(fontFamily, 11f, FontStyle.Regular, GraphicsUnit.Pixel);
            // var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            // foreach (var kv in LocalCodeToCoords)
            //     g.DrawString(kv.Key, smallFont, Brushes.Black, (int)(kv.Value.X - MinX + 30), (int)(kv.Value.Y - MinY + 30), format);
        }

        return btm;
    }
}
