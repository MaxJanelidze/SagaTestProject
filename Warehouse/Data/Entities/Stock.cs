namespace Warehouse.Data.Entities
{
    public class Stock
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ReservationStatus Status { get; set; }
    }

    public enum ReservationStatus
    {
        Free,
        Reserved
    }
}
