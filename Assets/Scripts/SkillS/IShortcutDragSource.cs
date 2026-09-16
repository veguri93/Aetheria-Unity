using UnityEngine;

public interface IShortcutDragSource
{
    ShortcutType ShortcutType { get; }

    int ShortcutReferenceId { get; }

    Sprite ShortcutIcon { get; }
}