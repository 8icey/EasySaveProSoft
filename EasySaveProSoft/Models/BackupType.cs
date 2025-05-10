using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySaveProSoft.Models
{
    // Enum representing the type of backup job
    public enum BackupType 
    {
        // Full backup: copies all files regardless of modification date
        Full,

        // Differential backup: only copies files modified since the last backup
        Differential
    }
}
