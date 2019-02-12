using System.ComponentModel.DataAnnotations.Schema;

namespace WebParser.Model
{
    [Table("pages")]
    public class Page: BaseModel
    {
        [ForeignKey("project_id")]
        public Project Project { get; set; }
    }
}