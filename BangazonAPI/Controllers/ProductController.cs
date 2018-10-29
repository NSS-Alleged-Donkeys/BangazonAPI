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

        // GET: api/<controller>
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
        [HttpGet("{id}")]
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
                return Ok(products);
            }
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
