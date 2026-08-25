using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerMovement Movement { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerTargeting Targeting { get; private set; }
    public PlayerInputController Input { get; private set; }

    public PlayerInteraction Interaction { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        Targeting = GetComponent<PlayerTargeting>();
        Input = GetComponent<PlayerInputController>();
        Interaction = GetComponent<PlayerInteraction>();
    }
}