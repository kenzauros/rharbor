using System.Collections.Generic;
using System.Linq;

namespace kenzauros.RHarbor
{
    internal class DesktopResolution
    {
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }

        public DesktopResolution(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
        }

        public override string ToString() => $"{Name} ({Width}x{Height})";

        private static readonly List<DesktopResolution> _List =
            new List<DesktopResolution>()
                {
                    new DesktopResolution("VGA", 640, 480),
                    new DesktopResolution("SVGA", 800, 600),
                    new DesktopResolution("XGA", 1024, 768),
                    new DesktopResolution("SXGA", 1280, 1024),
                    new DesktopResolution("WXGA", 1280, 800),
                    new DesktopResolution("SXGA+", 1400, 1050),
                    new DesktopResolution("UXGA", 1600, 1200),
                    new DesktopResolution("HD (Full HD)", 1920, 1080),
                    new DesktopResolution("WUXGA", 1920, 1200),
                    new DesktopResolution("WQHD", 2560, 1440),
                    new DesktopResolution("WQXGA", 2560, 1600),
                    new DesktopResolution("QUXGA", 3200, 2400),
                    new DesktopResolution("UHDTV (4K)", 3840, 2160),
                };
        public static List<DesktopResolution> List => _List;

        public static DesktopResolution Find(int width, int height)
            => List.FirstOrDefault(x => x.Width == width && x.Height == height);
    }
}
