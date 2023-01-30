using DiffHumanizer.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiffHumanizer.Test
{
    public enum TestEntityType { Supplier, Customer }
    public class TestClass
    {
        [Key]
        public int Id { get; set; }

        public TestEntityType EntityType { get; set; }

        [HumanizerEntityDescription]
        [Display(Name = "Description")]
        public string Name { get; set; }

        [Display(Name = "Some other property")]
        public string SomeOtherProperty { get; set; }

        [Display(Name = "Boolean property")]
        public bool BoolProperty { get; set; }

        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [HumanizerEntityName]
        public string HumanizedEntityName
        {
            get
            {
                if (EntityType == TestEntityType.Customer)
                    return "Customer";
                else
                    return "Supplier";
            }
        }

        [Display(Name = "Children 1")]
        public IList<ChildClass1> Children1 { get; set; }
       
        [Display(Name = "Children 2")]
        public IList<ChildClass2> Children2 { get; set; }
    }
}
