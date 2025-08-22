using UnityEngine;

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Game/Shop/Item Data")]
public class ItemData : ScriptableObject
{
    public ItemType Type;
    public Sprite Icon;
    public int Price;
    
    [Header("Skin to apply")]
    public SkinDefinition CharacterSkin;
}