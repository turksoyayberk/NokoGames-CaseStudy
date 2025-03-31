using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Game/Item")]
public class ItemSO : ScriptableObject
{
    public ItemType itemType;
    public Mesh mesh;
    public Material material;
}