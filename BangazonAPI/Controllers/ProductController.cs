using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using BangazonAPI.Models;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        // GET api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT 
                p.Id,
                p.ProductTypeId,
                p.CustomerId,
                p.Price,
                p.Title,
                p.Description,
                p.Quantity
            FROM Product P
            WHERE 1=1";

            if (q != null)
            {
                string isQ = $@"
                    AND p.Title LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Product> products = await conn.QueryAsync<Product>(sql);
                return Ok(products);
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
                SELECT 
                    p.Id,
                    p.ProductTypeId,
                    p.CustomerId,
                    p.Price,
                    p.Title,
                    p.Description,
                    p.Quantity
                FROM Product P
                WHERE p.Id = {id}";

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Product> products = await conn.QueryAsync<Product>(sql);
                return Ok(products.Single());
            }
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            string sql = $@"INSERT INTO Product 
            (ProductTypeId, CustomerId, Price, Title, Description, Quantity)
            VALUES
            (
                '{product.ProductTypeId}',
                '{product.CustomerId}',
                '{product.Price}',
                '{product.Title}',
                '{product.Description}',
                '{product.Quantity}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                product.Id = newId;
                return CreatedAtRoute("GetProduct", new { id = newId }, product);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            string sql = $@"
            UPDATE Product
            SET ProductTypeId = '{product.ProductTypeId}',
                CustomerId = '{product.CustomerId}',
                Price = '{product.Price}',
                Title = '{product.Title}',
                Description = '{product.Description}',
                Quantity = '{product.Quantity}'
            WHERE Id = {id}";

            try
            {
                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
            catch (Exception)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Product WHERE Id = {id}";

            using (IDbConnection conn = Connection)
            {
                int rowsAffected = await conn.ExecuteAsync(sql);
                if (rowsAffected > 0)
                {
                    return new StatusCodeResult(StatusCodes.Status204NoContent);
                }
                throw new Exception("No rows affected");
            }

        }

        private bool ProductExists(int id)
        {
            string sql = $"SELECT Id FROM Product WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Product>(sql).Count() > 0;
            }
        }
    }
}