using System.ComponentModel.DataAnnotations;

namespace DiffHumanizer.Test
{
    [Display(Name = "Child 1")]
    public class ChildClass1
    {
        [Key]
        public int Id { get; set; }
        
        [Display(Name = "Property 1")]
        public string Property1 { get; set; }
        
        [Display(Name = "Property 2")]
        public string Property2 { get; set; }
    }
}
