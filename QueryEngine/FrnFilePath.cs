using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QueryEngine
{
    internal class FrnFilePath
    {
        #region 
        private UInt64 _fileReferenceNumber;

        private UInt64? _parentFileReferenceNumber;

        private string _fileName;

        private string _path;
        #endregion

        public UInt64 FileReferenceNumber { get { return this._fileReferenceNumber; } }

        public UInt64? ParentFileReferenceNumber { get { return this._parentFileReferenceNumber; } }

        public string FileName { get { return this._fileName; } }

        public string Path
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
            }
        }

        public FrnFilePath(UInt64 fileReferenceNumber, UInt64? parentFileReferenceNumber, string fileName, string path = null)
        {
            this._fileReferenceNumber = fileReferenceNumber;
            this._parentFileReferenceNumber = parentFileReferenceNumber;
            this._fileName = fileName;
            this._path = path;
        }
    }
}
