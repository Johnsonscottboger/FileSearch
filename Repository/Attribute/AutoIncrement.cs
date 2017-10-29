using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Attribute
{
    /// <summary>
    /// 自动增长注解属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,AllowMultiple = false)]
    public class AutoIncrementAttribute: System.Attribute
    {

    }
}
