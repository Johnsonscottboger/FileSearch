using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Attribute
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class TableNameAttribute : System.Attribute
    {
        public string TableName { get; set; }
        public TableNameAttribute(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException("tableName");
            this.TableName = tableName;
        }
    }
}
