using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cursus.Entities
{
    public class Lesson : BaseEntity
    {
        public string Name { get; set; }
        public int No { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }
        public string VideoURL { get; set; }
        public string VideoFile { get; set; }
    }
}