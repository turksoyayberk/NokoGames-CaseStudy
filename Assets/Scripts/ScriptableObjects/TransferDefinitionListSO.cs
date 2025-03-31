using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TransferDefinition", menuName = "ScriptableObjects/Game/TransferDefinitionList")]
public class TransferDefinitionListSO : ScriptableObject
{
    public List<TransferDefinitionSO> transferDefinitionList;
}