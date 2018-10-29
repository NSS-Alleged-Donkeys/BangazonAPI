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

    public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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

        // GET api/ProductType?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT
                prodt.Id,
                prodt.Name
                
            FROM ProductType prodt
            WHERE 1=1
            ";

            if (q != null)
            {
                string isQ = $@"
                    AND prodt.Name LIKE '%{q}%'
                    
                ";
                sql = $"{sql} {isQ}";
            }

            Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<ProductType> productTypes = await conn.QueryAsync<ProductType>(sql);
                return Ok(productTypes);
            }
        }

        // GET api/ProductTypes/1
        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                prodt.Id,
                prodt.Name
            FROM ProductType prodt
            WHERE prodt.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<ProductType> productTypes = await conn.QueryAsync<ProductType>(sql);
                return Ok(productTypes.Single());
            }
        }

        // POST api/productType
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType productType)
        {
            string sql = $@"INSERT INTO ProductType 
            (Name)
            VALUES
            (
                '{productType.Name}'
             
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                productType.Id = newId;
                return CreatedAtRoute("GetProductType", new { id = newId }, productType);
            }
        }

        // PUT api/ProductType/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProductType productType)
        {
            string sql = $@"
            UPDATE ProductType
            SET Name = '{productType.Name}'  
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
                if (!ProductTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/productType/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM ProductType WHERE Id = {id}";

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

        private bool ProductTypeExists(int id)
        {
            string sql = $"SELECT Id FROM ProductType WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<ProductType>(sql).Count() > 0;
            }
        }
    }
}



