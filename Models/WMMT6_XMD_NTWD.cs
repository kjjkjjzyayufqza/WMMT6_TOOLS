using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace WMMT6_TOOLS.Models
{
    internal class WMMT6_XMD_NTWD
    {
        public byte[]? Magic {  get; set; }  //XMD   //offset = 0
        public byte[]? Ver1 { get; set; } // 0x30 0x30 0x31 0x00  = 001  //offset = 0x4

        public int Ver2 { get; set; } = 3; //好像都是0x3 //offset = 0x8

        public int FileCount { get; set; } //offset = 0xc

        public List<NTWD_FileData> NTWD_FileDatas { get; set; }
    }

    class NTWD_FileData
    {
        public int FileIndex { get; set; } // the file index in XMD
        public int FileStaffOffset { get; set; }
        public int FileSize { get; set; }
        public byte[]? FileData { get; set; }
    }
}
