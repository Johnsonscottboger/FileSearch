using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Attribute;

namespace FileSerach.Model
{
    public class SearchHistory
    {
        [Key]
        [Index]
        public string KeyWord { get; set; }

        public DateTime DateTime { get; set; }
    }
}
