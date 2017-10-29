using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UsnOperation;

namespace QueryEngine
{
    public class FileAndDirectoryEntry
    {
        protected UInt64 fileReferenceNumber;

        protected UInt64 parentFileReferenceNumber;

        protected string fileName;

        protected bool isFolder;

        protected bool isHidden;

        protected bool isSys;

        protected bool isNormal;

        protected string path;

        public UInt64 FileReferenceNumber
        {
            get
            {
                return this.fileReferenceNumber;
            }
        }

        public UInt64 ParentFileReferenceNumber
        {
            get
            {
                return this.parentFileReferenceNumber;
            }
        }

        public string FileName
        { 
            get 
            { 
                return this.fileName; 
            } 
        }

        public string Path
        {
            get
            {
                return this.path;
            }
        }

        public string FullFileName
        {
            get
            {
                return string.Concat(this.path, "\\", this.fileName);
            }
        }

        public bool IsFolder
        {
            get
            {
                return this.isFolder;
            }
        }

        public bool IsHidden
        {
            get { return this.isHidden; }
        }

        public bool IsSys
        {
            get { return this.isSys; }
        }

        public bool IsNormal
        {
            get { return this.isNormal; }
        }


        public FileAndDirectoryEntry(UsnEntry usnEntry, string path)
        {
            this.fileReferenceNumber = usnEntry.FileReferenceNumber;
            this.parentFileReferenceNumber = usnEntry.ParentFileReferenceNumber;
            this.fileName = usnEntry.FileName;
            this.isFolder = usnEntry.IsFolder;
            this.isHidden = usnEntry.IsHidden;
            this.isNormal = usnEntry.IsNormal;
            this.isSys = usnEntry.IsSys;
            this.path = path;
        }
    }
}
