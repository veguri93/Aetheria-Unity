using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    private readonly Dictionary<int, Monster> _monsters = new();

    private void Awake()
    {
        Instance = this;
    }

    public void AddMonster(Monster monster)
    {
        if (monster == null)
            return;

        _monsters[monster.ObjectId] = monster;
    }

    public bool TryGetMonster(
        int objectId,
        out Monster monster)
    {
        return _monsters.TryGetValue(
            objectId,
            out monster);
    }
}