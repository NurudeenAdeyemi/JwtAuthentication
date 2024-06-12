using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace productTestApi.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public List<Product> GetProducts()
        {
            var products = Product.Products;
            return products;
        }

        [HttpGet("{name}")]
        public IActionResult GetProduct(string name)
        {
            var product = Product.Products.SingleOrDefault(x => x.Name == name);
            return product != null? Ok(product) : NotFound();
        }

        [HttpPost]
        public IActionResult AddProduct([FromHeader] CreatProductModel model)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                Cost = model.Cost
            };
            Product.Products.Add(product);
            return CreatedAtAction(nameof(AddProduct), product);
        }
    }

    public class CreatProductModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
    }
}
