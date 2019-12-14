# Majesty-Gold-HD-Text-Tool
Convert Majesty Gold HD STR file to Text.

STR file struct.
4 bytes, Lines of this file.
2 bytes, mark of unicode. 
  English version is ushort 0x0000. 
  Chinese version is ushort 0x0800
n\*4 bytes, start offset of each line in this file.
n\*bytes or n\*4bytes, text of line. 
  For English version, string coded by ASCII, no start mark, end with 0x00. 
  For Chinese version, string coded by Unicode-LE, start with 0xFFFE, end with 0x0000
  
Converter will be ready soon. :D
