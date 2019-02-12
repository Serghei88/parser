using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebParser.Model
{
    [Table("projects")]
    public class Project: BaseModel
    {
        public virtual ICollection<Page> Pages { get; set; }
    }
}