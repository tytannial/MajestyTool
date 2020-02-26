# Majesty-Gold-HD-Tool

Extract Majesty Gold HD files

Pack files to CAM  

Convert Majesty Gold HD STR file to Text.

STR file struct.  
1. 4 bytes, Lines of this file. 
2. 2 bytes, mark of unicode.  
    * English version is ushort 0x0000.  
    * Chinese version is ushort 0x0800  
3. n\*4 bytes, start offset of each line in this file.  
4. n\*bytes or n\*4bytes, text of line.   
    * For English version, string coded by ASCII, no start mark, end with 0x00.   
    * For Chinese version, string coded by Unicode-LE, start with 0xFFFE, end with 0x0000  

Problem:  
1. Packing files need to be ordered. I can't find exactly orders for all files, except "gpltext" and "textdata". These two files' order can be found in "Doc" directory. Maybe these two header files are gived to Chinese publisher for locale.
2. Some files' names are messed, so can't be extracted normally.