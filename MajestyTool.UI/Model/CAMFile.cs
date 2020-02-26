namespace MajestyTool.UI.Model
{
    public class CAMFile
    {
        public int SectionCount { get; set; }
        public int ContentOffset { get; set; } // First file offset = 0x12(FileHeader) + 0x04(SectionCount) + 0x04(ContentOffset) + 0x08 * SectionCount
        public CAMSection[] Sections { get; set; }
    }
}
