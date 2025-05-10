using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveProSoft.Models
{
    // Represents a file that has been backed up (or attempted)
    public class FileItem 

    {
        // The original path of the file before backup
        public string SourcePath { get; set; }

        // The destination path of the file after being copied
        public string DestinationPath { get; set; }

        // The size of the file in bytes
        public long Size { get; set; }

        // Time taken to copy the file
        public TimeSpan TransferTime { get; set; }

        // Indicates whether the copy was successful
        public bool IsSuccess { get; set; }
    }
}
