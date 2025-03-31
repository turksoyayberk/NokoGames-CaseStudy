using UnityEngine;

[CreateAssetMenu(fileName = "TransferDefinition", menuName = "ScriptableObjects/Game/TransferDefinition")]
public class TransferDefinitionSO : ScriptableObject
{
    public LayerMask storageLayerMask;
    public TransferDirection direction;
    public ItemType itemType;
}

public enum TransferDirection
{
    FromStorageToCharacter,
    FromCharacterToStorage
}