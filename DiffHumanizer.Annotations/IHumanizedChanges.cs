namespace DiffHumanizer.Annotations
{
    public interface IHumanizedChanges<T> where T: class
    {
        string GetHumanizedChanges(T compareto);
    }
}
