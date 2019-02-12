using System.ComponentModel.DataAnnotations.Schema;

namespace WebParser.Model
{
    [Table("project_templates")]
    public class ProjectTemplate: BaseModel
    {
       [ForeignKey("project_id")]
        public Project Project { get; set; }
        
        [Column("is_deleted")]
        public int IsDeleted { get; set; }
    }
}