namespace MajestyTool.UI.Model
{
    public class CAMData
    {
        public int Offset { get; set; }
        public int Size { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }

        public override string ToString()
        {
            return FileName;
        }
    }
}
