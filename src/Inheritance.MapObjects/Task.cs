namespace Inheritance.MapObjects
{
    public interface IHasOwner
    {
        int Owner { get; set; }
    }

    public interface IHasArmy
    {
        Army Army { get; set; }
    }

    public interface IHasTreasure
    {
        Treasure Treasure { get; set; }
    }

    public class Dwelling : IHasOwner
    {
        public int Owner { get; set; }
    }

    public class Mine : IHasOwner, IHasArmy, IHasTreasure
    {
        public Army Army { get; set; }
        public int Owner { get; set; }
        public Treasure Treasure { get; set; }
    }

    public class Creeps : IHasArmy, IHasTreasure
    {
        public Army Army { get; set; }
        public Treasure Treasure { get; set; }
    }

    public class Wolves : IHasArmy
    {
        public Army Army { get; set; }
    }

    public class ResourcePile : IHasTreasure
    {
        public Treasure Treasure { get; set; }
    }

    public static class Interaction
    {
        public static void Make(Player player, object mapObject)
        {
            if (mapObject is IHasArmy army && !player.CanBeat(army.Army))
            {
                player.Die();

                return;
            }

            if (mapObject is IHasOwner owned) owned.Owner = player.Id;
            if (mapObject is IHasTreasure treasure) player.Consume(treasure.Treasure);
        }
    }
}