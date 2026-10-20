using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Aetheria/World Nameplate Style")]
public sealed class WorldNameplateStyle : ScriptableObject
{
    [Header("Name")]
    public TMP_FontAsset font;
    public Material fontMaterial;

    [Min(0.1f)]
    public float fontSize = 3.2f;

    public Color color =
        new Color(1f, 1f, 1f, 1f); // white

    [Header("Title")]
    [Min(0.1f)]
    public float titleFontSize = 2.0f;

    public Color titleColor =
        new Color(1f, 0.72f, 0.22f, 1f); // Lineage-like orange/gold

    [Min(0f)]
    public float titleVerticalSeparation = 0.32f;

    [Header("Position And Visibility")]
    [Min(0f)]
    public float verticalOffset = 0.6f;

    [Min(1f)]
    public float visibilityDistance = 35f;

    [Range(0f, 0.9f)]
    public float fadeDistanceFraction = 0.2f;
}
