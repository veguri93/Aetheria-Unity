public interface ITargetable
{
    int ObjectId { get; }

    string Name { get; }

    void Select();

    void Deselect();
}