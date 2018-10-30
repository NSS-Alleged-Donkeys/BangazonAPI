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
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
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

        // GET api/Customer?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT
                c.Id,
                c.FirstName,
                C.LastName
            FROM Customer c
            WHERE 1=1
            ";

            if (q != null)
            {
                string isQ = $@"
                    AND c.FirstName LIKE '%{q}%'
                    OR c.LastName LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }

            Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Customer> customer = await conn.QueryAsync<Customer>(sql);
                return Ok(customer);
            }
        }

        // GET api/Customer/5?_include=products
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute]int id, string _include)
        {
            string sql = $@"
            SELECT
                c.Id,
                c.FirstName,
                c.LastName
            FROM Customer c
            WHERE c.Id = {id}
            ";

            //If _include isn't in the route, this code won't be run
            if (_include != null)
            {
                //If the route has ?_include=products, this code will run
                if (_include == "products")
                {
                    //Here we are creating a new dictionary where the customer Id number will be the key. This way, we won't have multiple instances of each customer for each of their products.
                    Dictionary<int, Customer> report = new Dictionary<int, Customer>();

                    //This is our query to the database to get the customer and their products. We are starting with a customer and a product, putting them together, and returning a customer
                    IEnumerable<Customer> custAndProd = Connection.Query<Customer, Product, Customer>(
                       $@"
                    SELECT c.Id,
                        c.FirstName,
                        c.LastName,
                        p.Id,
                        p.Title,
                        p.Price,
                        p.Quantity,
                        p.Description,
                        p.ProductTypeId,
                        p.CustomerId
                    FROM Customer c
                    JOIN Product p ON c.Id = p.CustomerId
                    WHERE c.Id = {id};
                ",
                       //This logic gets run each time the query finds a match on the customer Id and the product's customerId. It's essentially a .map
                        (generatedCustomer, generatedProduct) => {
                            //If the report doesn't contain a key with the customer id, we create a new item in the dictionary 
                            if (!report.ContainsKey(generatedCustomer.Id))
                            {
                                report[generatedCustomer.Id] = generatedCustomer;
                            }

                            //Here we're adding the product to the customer's product list 
                            report[generatedCustomer.Id].Products.Add(generatedProduct);

                            //We are returning the generated customer here. We aren't using it anywhere, but we have to return something.
                            return generatedCustomer;
                        }
                    );

                    //If everything is successful, we are returning the dictionary that we built
                    return Ok(report);
                }

                if (_include == "payments")
                {
                    Dictionary<int, Customer> report = new Dictionary<int, Customer>();

                    IEnumerable<Customer> custAndPay = Connection.Query<Customer, PaymentType, Customer>(
                      $@"
                    SELECT c.Id,
                        c.FirstName,
                        c.LastName,
                        p.Id,
                        p.AcctNumber,
                        p.Name,
                        p.CustomerId
                    FROM Customer c
                    JOIN PaymentType p ON c.Id = p.CustomerId
                    WHERE c.Id = {id};
                ",
                      (generatedCustomer, generatedPaymentType) => {
                          if (!report.ContainsKey(generatedCustomer.Id))
                          {
                              report[generatedCustomer.Id] = generatedCustomer;
                          }

                          report[generatedCustomer.Id].PaymentTypes.Add(generatedPaymentType);

                          return generatedCustomer;
                         }
                      );

                    return Ok(report);
                }
            }

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Customer> customer = await conn.QueryAsync<Customer>(sql);
                return Ok(customer);
            }


        }

        // POST api/Customer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            string sql = $@"INSERT INTO Customer 
            (FirstName, LastName)
            VALUES
            (
                '{customer.FirstName}'
                ,'{customer.LastName}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                customer.Id = newId;
                return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
            }
        }

        // PUT api/Customer/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Customer customer)
        {
            string sql = $@"
            UPDATE Customer
            SET FirstName = '{customer.FirstName}',
                LastName = '{customer.LastName}'
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
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CustomerExists(int id)
        {
            string sql = $"SELECT Id FROM PaymentType WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<PaymentType>(sql).Count() > 0;
            }
        }
    }
}