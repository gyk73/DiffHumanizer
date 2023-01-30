# Diffhumanizer library

## Purpose

The diffhumanizer library can be used for deep comparison of two objects or two list of objects for changes and return the differences of any property of the entity or an aggregated entity or entity list in a human-readable string format.

For example you have a customer entity with a list of contact possibilities:

```
...
public class Customer
{
	[HumanizerEntityName]
	public string Name { get; set; }

    [Display("Contact possibility")]
	public IEnumerable<Contact> Contacts {get; set; }
}

public class Contact
{
	public string Type { get; set; }
	public string Value { get; set; }

    [Display("Default?")]
	public bool Default { get; set; }
}

...
```

If somebody added a new email address:

```
Modified Customer "Sample Customer" New Contact possibilitiy : Type : "E-Mail", Value : "someemailaddressgmail.com", Default? : "Yes"
```

If somebody modified the phone number:

```
Modified Customer "Sample Customer" Modified Contact possibilitiy : Type : "Phone", Value : "0670114455" => "0670114445"
```

Of the customer name was modified:

```
Modified Customer "Sample Customer" "Name": "Good Customer" => "Sample Customer"
```

## Usage

Just construct the instance of DifferenceHumanizer with possibly modified options and call the GetHumanizedPropertyDifferences method which returns the string:

```
var humanizer = new DifferenceHumanizer(new DifferenceHumanizerConfiguration());
var str = humanizer.GetHumanizedPropertyDifferences(entity1, entity2);

// Ready to use the differences string
Console.WriteLine(str);
```

## Annotations

For definig the human-readable property and object names use the "standard" DisplayAttribute from the System.ComponentModel.DataAnnotations namespace.

Beside of that attribute there are some Diffhumanizer-specific attributes:

- HumanizerIgnoreAttribute - for ignoring the property

- HumanizerEntityNameAttribute - for marking the property which holds the "Name" of the entity: the property based on that it can be easily identified later. If no such propertt found for the entity, the entity-s display name will be used (Such as "Customer") defined wit ha Display attribute. If no Display attribute exists for the entity then the entity class name will be used.
