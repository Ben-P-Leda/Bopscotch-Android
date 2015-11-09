namespace Bopscotch.Data
{
    public class SmashBlockItemData
    {
        public string TextureName;
        public int Count;
        public AffectedItem AffectsItem;
        public int Value;

        public enum AffectedItem
        {
            Score,
            PowerUp,
            GoldenTicket
        }
    }
}
