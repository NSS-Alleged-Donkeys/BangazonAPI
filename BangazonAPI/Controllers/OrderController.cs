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

        // GET api/Order?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string sql = @"
            SELECT
                o.Id,
                o.CustomerId,
                o.PaymentType
            FROM Order o
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
            FROM Order o
            WHERE o.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Order> orders = await conn.QueryAsync<Order>(sql);
                return Ok(orders);
            }
        }
    }
}
          /*
        // POST api/paymentType
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
        {
            string sql = $@"INSERT INTO PaymentType 
            (AcctNumber, Name, CustomerId)
            VALUES
            (
                '{paymentType.AcctNumber}'
                ,'{paymentType.Name}'
                ,'{paymentType.CustomerId}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                paymentType.Id = newId;
                return CreatedAtRoute("GetPayment", new { id = newId }, paymentType);
            }
        }

        // PUT api/paymentType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] PaymentType paymentType)
        {
            string sql = $@"
            UPDATE PaymentType
            SET AcctNumber = '{paymentType.AcctNumber}',
                Name = '{paymentType.Name}',
                CustomerId = '{paymentType.CustomerId}'
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
                if (!PaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/paymentType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM PaymentType WHERE Id = {id}";

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

        private bool PaymentTypeExists(int id)
        {
            string sql = $"SELECT Id FROM PaymentType WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<PaymentType>(sql).Count() > 0;
            }
        }
    }
}
*/
