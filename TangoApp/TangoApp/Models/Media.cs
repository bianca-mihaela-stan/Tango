using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TangoApp.Models
{
    public class Media
    {
        [Key]
        public int FileId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        //because we have an one-to-one relationship, we need to specify which end is principal in 
        //the relationship;  Principal end is the one which will be inserted first and which can exist
        //without the dependent one. Dependent end is the one which must be inserted after the principal
        //in our case, the principal entity is Post and the dependent one is Media
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}