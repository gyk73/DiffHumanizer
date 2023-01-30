namespace DiffHumanizer
{
    public class DifferenceHumanizerConfiguration
    {
        public string NewItemOperation { get; set; } = "New";
        public string ModifiedItemOperation { get; set; } = "Modified";
        public string DeletedItemOperation { get; set; } = "Deleted";
        public string TrueValue { get; set; } = "True";
        public string FalseValue { get; set; } = "False";
    }
}
