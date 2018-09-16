using System.Collections.Generic;
using System.Linq;

namespace kenzauros.RHarbor
{
    internal class DesktopResulution
    {
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }

        public DesktopResulution(string name, int width, int height)
        {
            Name = name;
            Width = width;
            Height = height;
        }

        public override string ToString() => $"{Name} ({Width}x{Height})";

        private static readonly List<DesktopResulution> _List =
            new List<DesktopResulution>()
                {
                    new DesktopResulution("VGA", 640, 480),
                    new DesktopResulution("SVGA", 800, 600),
                    new DesktopResulution("XGA", 1024, 768),
                    new DesktopResulution("SXGA", 1280, 1024),
                    new DesktopResulution("WXGA", 1280, 800),
                    new DesktopResulution("SXGA+", 1400, 1050),
                    new DesktopResulution("UXGA", 1600, 1200),
                    new DesktopResulution("HD (Full HD)", 1920, 1080),
                    new DesktopResulution("WUXGA", 1920, 1200),
                    new DesktopResulution("WQHD", 2560, 1440),
                    new DesktopResulution("WQXGA", 2560, 1600),
                    new DesktopResulution("QUXGA", 3200, 2400),
                    new DesktopResulution("UHDTV (4K)", 3840, 2160),
                };
        public static List<DesktopResulution> List => _List;

        public static DesktopResulution Find(int width, int height)
            => List.FirstOrDefault(x => x.Width == width && x.Height == height);
    }
}
