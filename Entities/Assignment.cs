using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Cursus.Entities
{
    public class Assignment : BaseEntity
    {
        public int No { get; set; }
        public string Title { get; set; }
        [Column(TypeName = "text")]
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Status { get; set; }
        public Guid SectionID { get; set; }
    }
}