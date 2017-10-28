using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSerach.Model
{
    /// <summary>
    /// 文件查找结果
    /// </summary>
    public class FileResult
    {
        public string Icon { get; set; }

        public string FileName { get; set; }

        public string FullName { get; set; }

        public string Size { get; set; }

        public DateTime? CreateDateTime { get; set; }
    }
}
