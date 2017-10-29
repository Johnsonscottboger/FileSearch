using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Attribute
{
    /// <summary>
    /// 索引注解属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IndexAttribute : System.Attribute
    {

    }
}
