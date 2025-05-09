using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveProSoft.Models
{
    public class FileItem
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public long Size { get; set; }
        public TimeSpan TransferTime { get; set; }
        public bool IsSuccess { get; set; }
    }
}

