using Fisobs.Core;
using UnityEngine;

namespace NPCSystem.Fisobs;

public class ItemIcon : Icon
{
    private readonly Item item;

    public ItemIcon(Item item)
    {
        this.item = item;
    }

    public override string SpriteName(int data)
    {
        return item.IconSprite;
    }

    public override Color SpriteColor(int data)
    {
        return item.IconColor;
    }

    public override int Data(AbstractPhysicalObject apo)
    {
        return 0;
    }
}