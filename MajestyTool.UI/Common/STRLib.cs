using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MajestyTool.UI.Model;

namespace MajestyTool.UI.Common
{
    public static class STRLib
    {
        public static STRFile Read(Stream fs)
        {
            using var reader = new BinaryReader(fs);

            var lineOffsets = new List<uint>();

            //How much text line in this file
            var length = reader.ReadUInt16();

            var unicodeFlag = reader.ReadByte();    //Unicode mode this flag will be 0x08, non unicode is 0x00

            var unkFlag = reader.ReadByte();       //Unkmark, 0x02 fixed

            //The line binary offset in this file
            for (int i = 0; i < length; ++i)
            {
                var filePos = reader.ReadUInt32();
                lineOffsets.Add(filePos);
            }

            var lines = new List<STRLine>();
            for (int i = 0; i < length; ++i)
            {
                var index = reader.ReadUInt32();
                int size;
                if (i + 1 == length)
                {
                    size = (int)(fs.Length - lineOffsets[i]);
                }
                else
                {
                    size = (int)(lineOffsets[i + 1] - lineOffsets[i]);
                }
                size -= 4;
                var code = reader.ReadBytes(size);
                string text;
                if (unicodeFlag == 0x08)
                {
                    text = Encoding.Unicode.GetString(code).Trim('\uFEFF', '\0'); // remove unicode mark and end mark
                }
                else
                {
                    text = Encoding.ASCII.GetString(code).Trim('\uFEFF', '\0'); // remove unicode mark and end mark
                }
                lines.Add(new STRLine { Index = index, Text = text });
            }

            var file = new STRFile
            {
                Length = length,
                UnicodeFlag = unicodeFlag,
                UnkFlag = unkFlag,
                LineOffsets = lineOffsets,
                Lines = lines
            };

            return file;
        }

        public static void Export(this STRFile strFile, string path, bool isUnicode)
        {
            using var fs = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(fs);

            writer.Write(strFile.Length);
            if (isUnicode)
            {
                writer.Write((byte)0x08);
            }
            else
            {
                writer.Write((byte)0x00);
            }
            writer.Write((byte)0x02);

            foreach (uint filePos in strFile.LineOffsets)
            {
                writer.Write(filePos);//offset
            }

            for (int i = 0; i < strFile.LineOffsets.Count; ++i)
            {
                writer.Write(i);    //line index
                byte[] code;
                if (isUnicode)
                {
                    code = Encoding.Unicode.GetBytes('\ufeff' + strFile.Lines[i].Text + '\0');
                }
                else
                {
                    code = Encoding.ASCII.GetBytes(strFile.Lines[i].Text + '\0');
                }
                writer.Write(code);
            }
        }
    }
}
