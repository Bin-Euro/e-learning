using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Entities
{
    public class Section : BaseEntity
    {
        public int? No { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public Guid CourseID { get; set; }
    }
}