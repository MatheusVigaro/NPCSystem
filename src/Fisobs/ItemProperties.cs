namespace NPCSystem.Fisobs;

public class ItemProperties : global::Fisobs.Properties.ItemProperties
{
    private readonly Item item;
    public ItemProperties(Item item)
    {
        this.item = item;
    }

    public override void Throwable(Player player, ref bool throwable)
    {
        throwable = item.Throwable;
    }

    public override void Nourishment(Player player, ref int quarterPips)
    {
        quarterPips = item.FoodPoints;
    }

    public override void ScavCollectScore(Scavenger scav, ref int score)
    {
        score = item.ScavScore;
    }

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = item.Grabability;
    }
}