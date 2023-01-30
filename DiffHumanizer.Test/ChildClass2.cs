using System.ComponentModel.DataAnnotations;

namespace DiffHumanizer.Test
{
    [Display(Name = "Child 2")]
    public class ChildClass2
    {
        [Key]
        public int Id { get; set; }

        [DiffHumanizer.Annotations.HumanizerEntityDescription]
        [Display(Name = "Property 1")]
        public string Property1 { get; set; }
        
        [Display(Name = "Property 2")]
        public string Property2 { get; set; }
    }
}
