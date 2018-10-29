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
    public class OrderController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrderController(IConfiguration config)
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

        //1. User should be able to GET a list, and GET a single item. DONE
        //2. When an order is deleted, every line item(i.e.entry in OrderProduct) should be removed
        //3. Should be able to filter out completed orders with the ?completed=false query string parameter.If the parameter value is true, then only completed order should be returned.
        //4. If the query string parameter of? _include = products is in the URL, then the list of products in the order should be returned.
        //5. If the query string parameter of? _include = customers is in the URL, then the customer representation should be included in the response.

        // GET api/Order?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string sql = @"
            SELECT
                o.Id,
                o.CustomerId,
                o.PaymentTypeId
            FROM [Order] o
            WHERE 1=1
            ";

            /*
            if (q != null)
            {
                string isQ = $@"
                    AND o.AcctNumber LIKE '%{q}%'
                    OR pt.Name LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }
            */
            Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Order> orders = await conn.QueryAsync<Order>(sql);
                return Ok(orders);
            }
        }

        // GET api/paymentTypes/5
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                o.Id,
                o.CustomerId,
                o.PaymentTypeId
            FROM [Order] o
            WHERE o.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Order> orders = await conn.QueryAsync<Order>(sql);
                return Ok(orders.Single());
            }
        }

        //GET WORKS
          
         //POST Works
        // POST api/paymentType
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            string sql = $@"INSERT INTO [Order] 
            (CustomerId, PaymentTypeId)
            VALUES
            (
                '{order.CustomerId}'
                ,'{order.PaymentTypeId}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                order.Id = newId;
                return CreatedAtRoute("GetOrder", new { id = newId }, order);
            }
        }

        // PUT api/paymentType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Order order)
        {
            string sql = $@"
            UPDATE [Order]
            SET CustomerId = '{order.CustomerId}',
                PaymentTypeId = '{order.PaymentTypeId}'
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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //2. When an order is deleted, every line item (i.e. entry in OrderProduct) should be removed
        // DELETE api/paymentType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM OrderProduct WHERE OrderId = {id};
                            DELETE FROM [Order] WHERE Id = {id};
                            ";

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

        private bool OrderExists(int id)
        {
            string sql = $"SELECT Id FROM [Order] WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Order>(sql).Count() > 0;
            }
        }
    }
}

