public class SystemMessage
{
    public int Id { get; }
    public string Name { get; }
    public string Text { get; }

    public SystemMessage(
        int id,
        string name,
        string text)
    {
        Id = id;
        Name = name;
        Text = text;
    }
}