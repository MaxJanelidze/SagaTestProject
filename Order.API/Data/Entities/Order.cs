namespace Order.API.Data.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public OrderStatus OrderStatus { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Rejcted,
        Approved
    }
}
