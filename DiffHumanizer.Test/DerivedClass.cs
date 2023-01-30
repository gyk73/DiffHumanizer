using System.ComponentModel.DataAnnotations;

namespace DiffHumanizer.Test
{
    public class DerivedClass: TestClass
    {
        [Display(Name = "Derived property")]
        public string DerivedProperty { get; set; }
    }
}
