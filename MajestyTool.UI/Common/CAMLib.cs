using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MajestyTool.UI.Model;

namespace MajestyTool.UI.Common
{
    public static class CAMLib
    {
        private static readonly byte[] FixHeader = new byte[] { 0x43, 0x59, 0x4C, 0x42, 0x50, 0x43, 0x20, 0x20, 0x01, 0x00, 0x01, 0x00 };

        /// <summary>
        /// Generate CAMFile object from a cam File
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static CAMFile Read(Stream fs)
        {
            using var reader = new BinaryReader(fs);

            //Skip file header
            reader.BaseStream.Seek(12, SeekOrigin.Begin);
            var camFile = new CAMFile();
            camFile.SectionCount = reader.ReadInt32(); // How many types.
            camFile.ContentOffset = reader.ReadInt32();// First file offset = 0x12(FileHeader) + 0x04(SectionCount) + 0x04(ContentOffset) + 0x04 * SectionCount + ContentOffset
            camFile.Sections = new CAMSection[camFile.SectionCount];

            for (int i = 0; i < camFile.Sections.Length; ++i)
            {
                var item = new CAMSection();
                var code = reader.ReadBytes(4);
                item.Extension = Encoding.ASCII.GetString(code);
                item.IndexOffset = reader.ReadInt32();
                camFile.Sections[i] = item;
            }

            for (int i = 0; i < camFile.Sections.Length; ++i)
            {
                var length = reader.ReadInt64(); // How many files with this Ext
                camFile.Sections[i].FilesData = new CAMData[length];
                for (int j = 0; j < length; ++j)
                {
                    var item = new CAMData();
                    var code = reader.ReadBytes(20);// voodoo
                    item.FileName = Encoding.ASCII.GetString(code);
                    item.Offset = reader.ReadInt32();
                    item.Size = reader.ReadInt32();

                    var current = fs.Position;// save position

                    item.Data = new byte[item.Size];
                    fs.Seek(item.Offset, SeekOrigin.Begin);
                    fs.Read(item.Data, 0, item.Size);

                    fs.Seek(current, SeekOrigin.Begin);

                    camFile.Sections[i].FilesData[j] = item;
                }
            }

            return camFile;
        }

        /// <summary>
        /// Pack all files in folder to a CAMFile Object
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static CAMFile Pack(string source)
        {
            var camFile = new CAMFile();
            var extSections = Directory.GetFiles(source).GroupBy(p => Path.GetExtension(p)).Reverse().ToArray();
            var sections = new CAMSection[extSections.Length];

            var totalFileCount = 0;
            var fileShift = 0;
            for (int i = 0; i < extSections.Length; ++i)
            {
                var ext = extSections[i];
                var section = new CAMSection();
                var list = ext.ToArray();
                section.IndexOffset = 12 + 4 + 4 + extSections.Length * 8 + i * 8 + fileShift * (20 + 4 + 4);
                section.Extension = ext.Key.TrimStart('.');
                section.FilesData = new CAMData[list.Length];
                for (int j = 0; j < section.FilesData.Length; ++j)
                {
                    var fs = new FileStream(list[j], FileMode.Open);
                    var data = new CAMData();
                    data.FileName = Path.GetFileNameWithoutExtension(list[j]);
                    data.Size = (int)fs.Length;
                    data.Data = new byte[fs.Length];
                    fs.Read(data.Data, 0, data.Size);
                    section.FilesData[j] = data;
                    ++totalFileCount;
                }
                sections[i] = section;
                fileShift += totalFileCount;
            }

            camFile.Sections = sections;
            camFile.SectionCount = extSections.Length;
            camFile.ContentOffset = extSections.Length * 8 + totalFileCount * (20 + 4 + 4); // SectionCount * 8(FileCount) + TotalFileCount * ( 20(text) + 4(offset) + 4(size) )

            var fileSizeShift = 0;
            foreach (var section in sections)
            {
                foreach (var fileData in section.FilesData)
                {
                    fileData.Offset = camFile.ContentOffset + fileSizeShift;
                    fileSizeShift += fileData.Size;
                }
            }

            return camFile;
        }

        /// <summary>
        /// Calculate all offsets order
        /// </summary>
        /// <param name="camFile"></param>
        public static void CalculateOffset(this CAMFile camFile)
        {
            var totalFileCount = 0;
            var fileShift = 0;
            var sectionLength = camFile.Sections.Length;

            for (int i = 0; i < sectionLength; ++i)
            {
                var section = camFile.Sections[i];
                section.IndexOffset = 12 + 4 + 4 + sectionLength * 8 + i * 8 + fileShift * (20 + 4 + 4);
                for (int j = 0; j < section.FilesData.Length; ++j)
                {
                    ++totalFileCount;
                }
                fileShift += totalFileCount;
            }

            camFile.SectionCount = sectionLength;
            camFile.ContentOffset = sectionLength * 8 + totalFileCount * (20 + 4 + 4);
        }

        /// <summary>
        /// Export all files in CAMFile Object to target path.
        /// </summary>
        /// <param name="camFile"></param>
        /// <param name="exportPath"></param>
        public static void Export(this CAMFile camFile, string exportPath)
        {
            Directory.CreateDirectory(exportPath);
            foreach (var section in camFile.Sections)
            {
                var ext = '.' + section.Extension;
                foreach (var file in section.FilesData)
                {
                    var filename = file.FileName.Replace("\0", "");
                    //TODO: FIX \u**** file
                    var path = Path.Combine(exportPath, filename + ext);
                    File.WriteAllBytes(path, file.Data);
                }
            }
        }

        /// <summary>
        /// Generate a cam file from a CAMFile object
        /// </summary>
        /// <param name="camFile"></param>
        /// <param name="path"></param>
        public static void Save(this CAMFile camFile, string path)
        {
            using var fs = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(fs, Encoding.ASCII);
            writer.Write(FixHeader);
            writer.Write(camFile.SectionCount);
            writer.Write(camFile.ContentOffset);
            foreach (var section in camFile.Sections)
            {
                var code = Encoding.ASCII.GetBytes(section.Extension);
                writer.Write(code);
                writer.Write(section.IndexOffset);
            }

            foreach (var section in camFile.Sections)
            {
                writer.Write((long)section.FilesData.Length);
                foreach (var file in section.FilesData)
                {
                    var code = Encoding.ASCII.GetBytes(file.FileName.PadRight(20, '\0'));
                    writer.Write(code);
                    writer.Write(file.Offset);
                    writer.Write(file.Size);
                }
            }

            foreach (var section in camFile.Sections)
            {
                foreach (var file in section.FilesData)
                {
                    writer.Write(file.Data);
                }
            }
        }
    }
}
