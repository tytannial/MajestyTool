using System.Collections.Generic;

namespace MajestyTool.UI.Model
{
    public class STRFile
    {
        public ushort Length { get; set; }
        public byte UnicodeFlag { get; set; }
        public byte UnkFlag { get; set; }
        public List<uint> LineOffsets { get; set; }
        public List<STRLine> Lines { get; set; }
    }
}
