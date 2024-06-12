namespace productTestApi
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Cost { get; set; }


        public static List<Product> Products = new List<Product>
        {
            new Product { Id = Guid.NewGuid(), Name = "Pen", Description = "For writing", Cost = 45m },
            new Product { Id = Guid.NewGuid(), Name = "Book", Description = "For School", Cost = 107.5m },
            new Product { Id = Guid.NewGuid(), Name = "Phone", Description = "For calling", Cost = 1345m }
        };
    }
}
