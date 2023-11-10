using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text.Json;
using Clipper2Lib;

const string popzonePath = @".\popzone.ipl";
var content = File.ReadAllText(popzonePath).Replace("\r", "");
var lines = content.Split("\n").Where(x => x.Contains(' ')).ToArray();

var cuboids = new List<Cuboid>();

foreach (var line in lines)
{
    var words = line.Split(",").Select(x => x.Trim()).ToArray();
    words[0] = words[0].ToUpper();
    words[7] = words[7].ToUpper();
    var cuboid = new Cuboid
    {
        LocalCode = words[0],
        X1 = double.Parse(words[1]),
        Y1 = double.Parse(words[2]),
        Z1 = double.Parse(words[3]),
        X2 = double.Parse(words[4]),
        Y2 = double.Parse(words[5]),
        Z2 = double.Parse(words[6]),
        GlobalCode = words[7]
    };

    if (cuboid.LocalCode.StartsWith("FXT"))
        continue;
    if (cuboid.LocalCode.StartsWith("VFX"))
        continue;

    cuboids.Add(cuboid);
}

// Just to render map right side up
// var minY = cuboids.Select(x => Math.Min(x.Y1, x.Y2)).Min();
// var maxY = cuboids.Select(x => Math.Max(x.Y1, x.Y2)).Max();
// var height = maxY - minY;
// foreach (var cuboid in cuboids)
// {
//     cuboid.Y1 = height - (cuboid.Y1 - minY);
//     cuboid.Y2 = height - (cuboid.Y2 - minY);
// }

var codeToZone = new Dictionary<string, Zone>();

for (int i = 0; i < cuboids.Count; i++)
{
    Console.Write($"\r{i}/{cuboids.Count}");

    var cuboid = cuboids[i];

    if (!codeToZone.ContainsKey(cuboid.GlobalCode))
        codeToZone[cuboid.GlobalCode] = new() { Code = cuboid.GlobalCode };
    codeToZone[cuboid.GlobalCode].AddCuboid(cuboid);
}
Console.WriteLine($"\r{cuboids.Count}/{cuboids.Count}");

#region Conflict Solver
if (codeToZone.TryGetValue("OCEANA", out var oceana))
{
    var zones = codeToZone.Values.ToArray();
    for (int i = 0; i < zones.Length; i++)
    {
        Console.Write($"\r{i}/{zones.Length}");
        zones[i].SubtractZone(oceana);
    }
    Console.WriteLine($"\r{zones.Length}/{zones.Length}");
}

codeToZone.Remove("BAYTRE");

var clipCuboidFromZone = new List<(string subject, string clip)>
{
    ("ALAMO", "CALAB"),
    ("ALAMO", "C_CRE5"),
    ("CMSW", "CIDE144"),
    ("LAGO", "ARMY5"),
    ("MIRR", "FRW5"),
    ("MTCHIL", "C_CRE4"),
    ("MTCHIL", "CIDE63"),
    ("NCHU", "FRW63"),
    ("PALHIGH", "NOSE2"),
    ("CYPRE", "RANC5"),
    ("ROCKF", "Z_MVS6"),
    ("SKID", "LEGS3"),
};

foreach (var tuple in clipCuboidFromZone)
{
    if (!codeToZone.TryGetValue(tuple.subject, out var subject))
        continue;

    var clip = cuboids.FirstOrDefault(x => x.LocalCode == tuple.clip);
    if (clip is null)
        continue;

    subject.SubtractCuboid(clip);
}

var clipZoneFromZone = new List<(string subject, string clip)>
{
    ("ALAMO", "GALFISH"),
    ("BRADP", "BRADT"),
    ("CANNY", "CCREAK"),
    ("CHIL", "GALLI"),
    ("CHIL", "OBSERV"),
    ("PBOX", "DOWNT"),
    ("GREATC", "ZANCUDO"),
    ("ARMYB", "LAGO"),
    ("ARMYB", "NCHU"),
    ("LMESA", "RANCHO"),
    ("PALFOR", "PALCOV"),
    ("PBOX", "LEGSQU"),
    ("SANDY", "ALAMO"),
    ("STAD", "ELYSIAN"),
    ("TATAMO", "HORS"),
    ("TATAMO", "LACT"),
    ("TATAMO", "LDAM"),
    ("TATAMO", "NOOSE"),
};

foreach (var tuple in clipZoneFromZone)
{
    if (!codeToZone.TryGetValue(tuple.subject, out var subject))
        continue;

    if (!codeToZone.TryGetValue(tuple.clip, out var clip))
        continue;

    subject.SubtractZone(clip);
}
#endregion

var codeToName = new Dictionary<string, string>
{
    ["AIRP"] = "Los Santos International Airport",
    ["ALAMO"] = "Alamo Sea",
    ["ALTA"] = "Alta",
    ["ARMYB"] = "Fort Zancudo",
    ["BANHAMCA"] = "Banham Canyon Drive",
    ["BANNING"] = "Banning",
    ["BAYTRE"] = "Baytree Canyon",
    ["BEACH"] = "Vespucci Beach",
    ["BHAMCA"] = "Banham Canyon",
    ["BRADP"] = "Braddock Pass",
    ["BRADT"] = "Braddock Tunnel",
    ["BURTON"] = "Burton",
    ["CALAFB"] = "Calafia Bridge",
    ["CANNY"] = "Raton Canyon",
    ["CCREAK"] = "Cassidy Creek",
    ["CHAMH"] = "Chamberlain Hills",
    ["CHIL"] = "Vinewood Hills",
    ["CHU"] = "Chumash",
    ["CMSW"] = "Chiliad Mountain State Wilderness",
    ["COSI"] = "Countryside",
    ["CYPRE"] = "Cypress Flats",
    ["DAVIS"] = "Davis",
    ["DELBE"] = "Del Perro Beach",
    ["DELPE"] = "Del Perro",
    ["DELSOL"] = "La Puerta",
    ["DESRT"] = "Grand Senora Desert",
    ["DOWNT"] = "Downtown",
    ["DTVINE"] = "Downtown Vinewood",
    ["EAST_V"] = "East Vinewood",
    ["EBURO"] = "El Burro Heights",
    ["ECLIPS"] = "Eclipse",
    ["ELGORL"] = "El Gordo Lighthouse",
    ["ELSANT"] = "East Los Santos",
    ["ELYSIAN"] = "Elysian Island",
    ["GALFISH"] = "Galilee",
    ["GALLI"] = "Galileo Park",
    ["GOLF"] = "GWC and Golfing Society",
    ["GRAPES"] = "Grapeseed",
    ["GREATC"] = "Great Chaparral",
    ["HARMO"] = "Harmony",
    ["HAWICK"] = "Hawick",
    ["HEART"] = "Heart Attacks Beach",
    ["HORS"] = "Vinewood Racetrack",
    ["HUMLAB"] = "Humane Labs and Research",
    ["JAIL"] = "Bolingbroke Penitentiary",
    ["KOREAT"] = "Little Seoul",
    ["LACT"] = "Land Act Reservoir",
    ["LAGO"] = "Lago Zancudo",
    ["LDAM"] = "Land Act Dam",
    ["LEGSQU"] = "Legion Square",
    ["LMESA"] = "La Mesa",
    ["LOSPUER"] = "La Puerta",
    ["LOSSF"] = "Los Santos Freeway",
    ["MIRR"] = "Mirror Park",
    ["MORN"] = "Morningwood",
    ["MOVIE"] = "Richards Majestic",
    ["MTCHIL"] = "Mount Chiliad",
    ["MTGORDO"] = "Mount Gordo",
    ["MTJOSE"] = "Mount Josiah",
    ["MURRI"] = "Murrieta Heights",
    ["NCHU"] = "North Chumash",
    ["NOOSE"] = "N.O.O.S.E",
    ["OBSERV"] = "Galileo Observatory",
    ["OCEANA"] = "Pacific Ocean",
    ["PALCOV"] = "Paleto Cove",
    ["PALETO"] = "Paleto Bay",
    ["PALFOR"] = "Paleto Forest",
    ["PALHIGH"] = "Palomino Highlands",
    ["PALMPOW"] = "Palmer-Taylor Power Station",
    ["PBLUFF"] = "Pacific Bluffs",
    ["PBOX"] = "Pillbox Hill",
    ["PROCOB"] = "Procopio Beach",
    ["PROL"] = "North Yankton",
    ["RANCHO"] = "Rancho",
    ["RGLEN"] = "Richman Glen",
    ["RICHM"] = "Richman",
    ["ROCKF"] = "Rockford Hills",
    ["RTRAK"] = "Redwood Lights Track",
    ["SANAND"] = "San Andreas",
    ["SANCHIA"] = "San Chianski Mountain Range",
    ["SANDY"] = "Sandy Shores",
    ["SKID"] = "Mission Row",
    ["SLAB"] = "Stab City",
    ["SLSANT"] = "South Los Santos",
    ["STAD"] = "Maze Bank Arena",
    ["STRAW"] = "Strawberry",
    ["TATAMO"] = "Tataviam Mountains",
    ["TERMINA"] = "Terminal",
    ["TEXTI"] = "Textile City",
    ["TONGVAH"] = "Tongva Hills",
    ["TONGVAV"] = "Tongva Valley",
    ["UTOPIAG"] = "Utopia Gardens",
    ["VCANA"] = "Vespucci Canals",
    ["VESP"] = "Vespucci",
    ["VINE"] = "Vinewood",
    ["WINDF"] = "Ron Alternates Wind Farm",
    ["WMIRROR"] = "West Mirror Drive",
    ["WVINE"] = "West Vinewood",
    ["ZANCUDO"] = "Zancudo River",
    ["ZENORA"] = "Senora Freeway",
    ["ZP_ORT"] = "Port of South Los Santos",
    ["ZQ_UAR"] = "Davis Quartz",
};

var missingNames = codeToZone.Keys.Except(codeToName.Keys);
if (missingNames.Any())
{
    Console.WriteLine("Missing names: " + string.Join(", ", missingNames));

    // var zones = missingNames.Select(x => codeToZone[x]).ToArray();
    // RenderMap(codeToZone.Values.Where(x => x.Code != "PROL" && x.Code != "OCEANA").ToArray(), zones, renderZoneNames: true);
}
// else
// RenderMap(codeToZone.Values.Where(x => x.Code != "PROL" && x.Code != "OCEANA").ToArray(), renderZoneNames: true);

// RenderZone(codeToZone, "OCEANA");
// RenderConflicts(codeToZone.Values.Where(x => x.Code != "PROL" && x.Code != "OCEANA").ToArray());

GenerateDataJson(codeToZone.Values.Where(x => x.Code != "PROL" && x.Code != "OCEANA").ToArray());

void RenderConflicts(Zone[] zones)
{
    if (Directory.Exists(@".\output\conflicts"))
        Directory.Delete(@".\output\conflicts", true);
    Directory.CreateDirectory(@".\output\conflicts");

    using var fontsCollection = new InstalledFontCollection();
    var fontFamily = fontsCollection.Families.FirstOrDefault(x => x.Name == "Cascadia Code") ?? fontsCollection.Families.FirstOrDefault();
    Font? font = null;
    if (fontFamily is not null)
        font = new Font(fontFamily, 18f, FontStyle.Regular, GraphicsUnit.Pixel);
    var formatCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

    for (int i = 0; i < zones.Length - 1; i++)
    {
        Console.Write($"\r{i}/{zones.Length - 1}");
        for (int j = i + 1; j < zones.Length; j++)
        {
            var intersect = Clipper.Intersect(zones[i].Polygon, zones[j].Polygon, FillRule.NonZero, 4);
            if (intersect.Count == 0)
                continue;

            var minX = Math.Min(zones[i].MinX, zones[j].MinX);
            var minY = Math.Min(zones[i].MinY, zones[j].MinY);
            var maxX = Math.Max(zones[i].MaxX, zones[j].MaxX);
            var maxY = Math.Max(zones[i].MaxY, zones[j].MaxY);

            using var btm = new Bitmap((int)(maxX - minX + 60), (int)(maxY - minY + 60));
            using var g = Graphics.FromImage(btm);
            g.Clear(Color.White);

            foreach (var path in zones[i].Polygon)
            {
                var points = path.Select(x => new PointF((float)(x.x - minX + 30), (float)(x.y - minY + 30))).ToArray();
                g.DrawPolygon(Pens.Black, points);
            }

            foreach (var path in zones[j].Polygon)
            {
                var points = path.Select(x => new PointF((float)(x.x - minX + 30), (float)(x.y - minY + 30))).ToArray();
                g.DrawPolygon(Pens.Black, points);
            }

            foreach (var path in intersect)
            {
                var points = path.Select(x => new PointF((float)(x.x - minX + 30), (float)(x.y - minY + 30))).ToArray();
                g.FillPolygon(new SolidBrush(Color.FromArgb(0x80, 0xFF, 0, 0)), points);
                g.DrawPolygon(Pens.Red, points);
            }

            foreach (var cuboid in zones[i].Cuboids)
            {
                var x1 = Math.Min(cuboid.X1, cuboid.X2) - minX + 32;
                var y1 = Math.Min(cuboid.Y1, cuboid.Y2) - minY + 32;
                var x2 = Math.Max(cuboid.X1, cuboid.X2) - minX + 28;
                var y2 = Math.Max(cuboid.Y1, cuboid.Y2) - minY + 28;

                var pen = new Pen(Color.FromArgb((int)(cuboid.LocalCode.GetHashCode() | 0xFF000000)));
                g.DrawRectangle(pen, (int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));

                if (font is not null)
                {
                    var centerX = (x1 + x2) / 2;
                    var centerY = (y1 + y2) / 2;

                    g.DrawString(cuboid.LocalCode, font, Brushes.Orange, (int)centerX, (int)centerY, formatCenter);
                }
            }

            foreach (var cuboid in zones[j].Cuboids)
            {
                var x1 = Math.Min(cuboid.X1, cuboid.X2) - minX + 32;
                var y1 = Math.Min(cuboid.Y1, cuboid.Y2) - minY + 32;
                var x2 = Math.Max(cuboid.X1, cuboid.X2) - minX + 28;
                var y2 = Math.Max(cuboid.Y1, cuboid.Y2) - minY + 28;

                var pen = new Pen(Color.FromArgb((int)(cuboid.LocalCode.GetHashCode() | 0xFF000000)));
                g.DrawRectangle(pen, (int)x1, (int)y1, (int)(x2 - x1), (int)(y2 - y1));

                if (font is not null)
                {
                    var centerX = (x1 + x2) / 2;
                    var centerY = (y1 + y2) / 2;

                    g.DrawString(cuboid.LocalCode, font, Brushes.Cyan, (int)centerX, (int)centerY, formatCenter);
                }
            }

            btm.Save($@".\output\conflicts\{zones[i].Code} - {zones[j].Code}.webp", ImageFormat.Webp);
        }
    }
    Console.WriteLine($"\r{zones.Length - 1}/{zones.Length - 1}");
}

void RenderMap(Zone[] zones, Zone[]? toHighlight = null, bool renderZoneNames = false, bool renderGlobalCode = false, bool renderLocalCode = false)
{
    var minX = zones.Min(x => x.MinX);
    var minY = zones.Min(x => x.MinY);
    var maxX = zones.Max(x => x.MaxX);
    var maxY = zones.Max(x => x.MaxY);

    using var btm = new Bitmap((int)(maxX - minX + 60), (int)(maxY - minY + 60));
    using var g = Graphics.FromImage(btm);
    g.Clear(Color.White);

    using var fontsCollection = new InstalledFontCollection();
    var fontFamily = fontsCollection.Families.FirstOrDefault(x => x.Name == "Cascadia Code") ?? fontsCollection.Families.FirstOrDefault();
    Font? font = null;
    if (fontFamily is not null)
        font = new Font(fontFamily, 18f, FontStyle.Regular, GraphicsUnit.Pixel);
    var formatCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

    foreach (var zone in zones)
    {
        var color = Color.FromArgb((int)(zone.Code.GetHashCode() & 0x00FFFFFF | 0x80000000));
        var brush = new SolidBrush(color);

        foreach (var path in zone.Polygon)
        {
            var points = path.Select(x => new PointF((float)(x.x - minX + 30), (float)(x.y - minY + 30))).ToArray();
            if (toHighlight is null || toHighlight.Contains(zone))
                g.FillPolygon(brush, points);
            g.DrawPolygon(Pens.Black, points);
        }

        if (font is not null)
        {
            if (renderZoneNames || renderGlobalCode)
            {
                var centerX = (zone.MinX + zone.MaxX) / 2 - minX + 30;
                var centerY = (zone.MinY + zone.MaxY) / 2 - minY + 30;

                var str = renderZoneNames && codeToName is not null && codeToName.TryGetValue(zone.Code, out var strBuff) ? strBuff : zone.Code;
                g.DrawString(str, font, Brushes.Black, (int)centerX, (int)centerY, formatCenter);
            }

            if (renderLocalCode)
            {
                foreach (var cuboid in zone.Cuboids)
                {
                    var centerX = (cuboid.X1 + cuboid.X2) / 2 - minX + 30;
                    var centerY = (cuboid.Y1 + cuboid.Y2) / 2 - minY + 30;

                    g.DrawString(cuboid.LocalCode, font, Brushes.Black, (int)centerX, (int)centerY, formatCenter);
                }
            }
        }
    }

    if (!Directory.Exists(@".\output"))
        Directory.CreateDirectory(@".\output");
    btm.Save($@".\output\map.webp", ImageFormat.Webp);
}

void RenderZone(Dictionary<string, Zone> dict, string zoneCode)
{
    if (dict.TryGetValue(zoneCode, out var zone))
    {
        if (!Directory.Exists(@".\output"))
            Directory.CreateDirectory(@".\output");

        using var btm = zone.DrawLines();
        btm.Save($@".\output\{zone.Code}.webp", ImageFormat.Webp);
    }
}

void GenerateDataJson(Zone[] zones)
{
    var minX = zones.Min(x => x.MinX);
    var minY = zones.Min(x => x.MinY);
    var maxX = zones.Max(x => x.MaxX);
    var maxY = zones.Max(x => x.MaxY);

    var width = maxX - minX;
    var height = maxY - minY;

    const int gridColumns = 16;
    const int gridRows = 16;

    var gridCellWidth = width / gridColumns;
    var gridCellHeight = height / gridRows;

    var grid = new DataGridCell[gridRows][];

    for (int iy = 0; iy < gridRows; iy++)
    {
        grid[iy] = new DataGridCell[gridColumns];

        for (int ix = 0; ix < gridColumns; ix++)
        {
            var x1 = minX + gridCellWidth * ix;
            var y1 = minY + gridCellHeight * iy;
            var x2 = x1 + gridCellWidth;
            var y2 = y1 + gridCellHeight;

            var path = new PathD();
            path.Add(new PointD(x1, y1));
            path.Add(new PointD(x1, y2));
            path.Add(new PointD(x2, y2));
            path.Add(new PointD(x2, y1));

            var gridPolygon = new PathsD();
            gridPolygon.Add(path);

            var zoneCodes = new List<string>();
            foreach (var zone in zones)
            {
                var intersect = Clipper.Intersect(zone.Polygon, gridPolygon, FillRule.NonZero, 4);
                if (intersect.Count == 0)
                    continue;

                zoneCodes.Add(zone.Code);
            }

            grid[iy][ix] = new()
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Column = ix,
                Row = iy,
                ZoneCodes = zoneCodes
            };
        }
    }

    var dataZones = new List<DataZone>();
    foreach (var zone in zones)
    {
        dataZones.Add(new()
        {
            Code = zone.Code,
            Name = codeToName is not null && codeToName.TryGetValue(zone.Code, out var name) ? name : zone.Code,
            Polygons = zone.Polygon.Select(x => x.Select(xx => new DataPoint { X = xx.x, Y = xx.y }).ToArray()).ToArray()
        });
    }

    var data = new Data
    {
        GridLookup = grid,
        Zones = dataZones
    };

    var json = JsonSerializer.Serialize(data);

    if (!Directory.Exists(@".\output"))
        Directory.CreateDirectory(@".\output");
    File.WriteAllText(@".\output\data.json", json);
}
