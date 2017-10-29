using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace FileSerach.Model
{
    /// <summary>
    /// 文件查找结果
    /// </summary>
    public class FileResult
    {
        public ImageSource Icon { get; set; }

        public string FileName { get; set; }

        public string FullName { get; set; }

        public string Size { get; set; }

        public bool IsFolder { get; set; }

        public bool IsHidden { get; set; }
        public bool IsSys { get; set; }

        public bool IsNormal { get; set; }

        public DateTime? CreateDateTime { get; set; }
    }
}
