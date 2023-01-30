using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DiffHumanizer.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Added()
        {
            var entity1 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", BoolProperty = true,
                                   Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "Property 2" } } };
            TestClass entity0 = null; // new TestClass ();

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration());
            var str = humanizer.GetHumanizedPropertyDifferences(entity0, entity1);

            Console.WriteLine(str);

            Assert.IsTrue(str == "New Supplier \"This is the name\" Description : \"This is the name\", Some other property : \"Other data\", Boolean property : \"True\", Quantity : \"0\", Children 1 : Property 1 : \"Property 1\", Property 2 : \"Property 2\"");
        }

        [TestMethod]
        public void Change()
        {
            var entity1 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", BoolProperty = false, Quantity = 1M };
            var entity2 = new TestClass { Id = 1, Name = "This is the name modified", SomeOtherProperty = "Other data changed", BoolProperty = true, Quantity = 1.4M };

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration());
            var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);

            Console.WriteLine(str);

            Assert.IsTrue(str == "Modified Supplier \"This is the name\" Description : \"This is the name\" => \"This is the name modified\", Some other property : \"Other data\" => \"Other data changed\", Boolean property : \"False\" => \"True\", Quantity : \"1\" => \"1.4\"");
        }

        [TestMethod]
        public void Deleted()
        {
            var entity1 = new TestClass
            {
                Id = 1,
                Name = "This is the name",
                SomeOtherProperty = "Other data",
                Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "Property 2" } }
            };
            TestClass entity2 = null; // new TestClass ();

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration());
            var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);

            Console.WriteLine(str);

            Assert.IsTrue(str == "Deleted Supplier \"This is the name\" Description : \"This is the name\", Some other property : \"Other data\", Boolean property : \"False\", Quantity : \"0\", Children 1 : Property 1 : \"Property 1\", Property 2 : \"Property 2\"");
        }

        [TestMethod]
        public void ChildAdded()
        {
            var entity1 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1>()};
            var entity2 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "Property 2" } } };

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration());
            var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);

            Console.WriteLine(str);

            Assert.IsTrue(str == "Modified Supplier \"This is the name\" New Children 1 : Property 1 : \"Property 1\", Property 2 : \"Property 2\"");
        }
     
        [TestMethod]
        public void ChilModified()
        {
            var entity1 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "Property 2" } }, Children2 = new List<ChildClass2>() };
            var entity2 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1 changed", Property2 = "Property 2 changed" } } };

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration());
            var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);
            
            Console.WriteLine(str);

            Assert.IsTrue(str == "Modified Supplier \"This is the name\" Modified Children 1 : Property 1 : \"Property 1\" => \"Property 1 changed\", Property 2 : \"Property 2\" => \"Property 2 changed\"");
        }

        [TestMethod]
        public void ChilDeleted()
        {
            var entity1 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "Property 2" } } };
            var entity2 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> () };

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration());
            var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);

            Console.WriteLine(str);

            Assert.IsTrue(str == "Modified Supplier \"This is the name\" Deleted Children 1 : Property 1 : \"Property 1\", Property 2 : \"Property 2\"");
        }

        [TestMethod]
        public void NotChanged()
        {
            var entity1 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "<Not selected>" } }, Children2 = new List<ChildClass2>() };
            var entity2 = new TestClass { Id = 1, Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = null } }, Children2 = new List<ChildClass2>() };

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration(), new Dictionary<string, string>() { { "<Not selected>", "null" } });
            humanizer.ReturnNullWhenNoChanges = true;
            var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);

            //Console.WriteLine(str);

            Assert.IsTrue(string.IsNullOrEmpty(str));
        }

        [TestMethod]
        public void DerivedClass()
        {
            var entity1 = new DerivedClass { Id = 1, DerivedProperty = "Derived prop. value", Name = "This is the name", SomeOtherProperty = "Other data", Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "<Not selected>" } }, Children2 = new List<ChildClass2>() };
            var entity2 = new DerivedClass { Id = 1, DerivedProperty = "Derived prop. changed", Name = "This is the name", SomeOtherProperty = null, Children1 = new List<ChildClass1> { new ChildClass1 { Id = 1, Property1 = "Property 1", Property2 = "" } }, Children2 = new List<ChildClass2>() };

            var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration(), new Dictionary<string, string>() { { "<Not selected>", "" } });
            humanizer.ReturnNullWhenNoChanges = true;
            var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);

            Console.WriteLine(str);
            Assert.IsTrue(str == "Modified Supplier \"This is the name\" Derived property : \"Derived prop. value\" => \"Derived prop. changed\", Some other property : \"Other data\" => \"null\"");
        }        
    }
}
