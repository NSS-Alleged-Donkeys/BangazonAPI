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

/* Authored By:  Helen Chalmers
 
          Purpose: To allow Developers to access the ORders Table in the Bangazon API DB - Developers should be able to 
            * Get All of the Order
            * Get One Order
            * Get an Order with it's products
            * Get an order with a customer listed
            * Post (create) an Order
            * Put(edit) an Order
            * Delete an Order and the associated prodcuts on the order (not the products themselves)
 
        1. User should be able to GET a list, and GET a single item. DONE
        2. When an order is deleted, every line item(i.e.entry in OrderProduct) should be removed DONE
        3. Should be able to filter out completed orders with the ?completed=false query string parameter.If the parameter value is true, then only completed order should be returned.
        4. If the query string parameter of? _include = products is in the URL, then the list of products in the order should be returned.
        5. If the query string parameter of? _include = customer is in the URL, then the customer representation should be included in the response.

*/
namespace BangazonAPI.Controllers

//Sets the route and _config variable for the Database Connection
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


        // GET -- Returns all orders - api/Order?q=Taco 
        [HttpGet]
        public async Task<IActionResult> Get(string completed, string _include)
        {
        //3. Should be able to filter out completed orders with the ?completed=false query string parameter.If the parameter value is true, then only completed order should be returned.
            string sql = $"Select * FROM [Order]";
            using (IDbConnection conn = Connection)
                if (completed == "false")
                {
                    sql += $"WHERE [Order].PaymentTypeId IS NULL";
                    Console.WriteLine(sql);
                    var NotCompletedOrder = await conn.QueryAsync<Order>(sql);
                    return Ok(NotCompletedOrder);

                }
                else if (completed == "true")
                {
                    sql += $"WHERE [Order].PaymentTypeId IS NOT NULL";
                    Console.WriteLine(sql);
                    var CompletedOrder = await conn.QueryAsync<Order>(sql);
                    return Ok(CompletedOrder);
                }

            if (_include != null)
            {
                //4. If the query string parameter of? _include = products is in the URL, then the list of products in the order should be returned.
                if (_include == "products")
                {
                    Dictionary<int, Order> products = new Dictionary<int, Order>();
                    IEnumerable<Order> OrdandOrdProdandProd = Connection.Query<Order, Product, Order>(
                        $@"SELECT o.Id,
                            o.CustomerId,
                            o.PaymentTypeId,
                            op.Id, 
                            op.OrderId,
                            op.ProductId,
                            p.Id,
                            p.Title
                        FROM Product p
                        JOIN OrderProduct op ON p.Id = op.ProductId
                        JOIN [Order] o ON o.Id = op.OrderId  
                         WHERE 1 = 1; 
                        ",

                        (generatedOrder, generatedProduct) =>
                        {
                            if (!products.ContainsKey(generatedOrder.Id))
                            {
                                products[generatedOrder.Id] = generatedOrder;
                            }

                            products[generatedOrder.Id].productList.Add(generatedProduct);
                            return generatedOrder;
                        }

                        );
                    return Ok(products);
                }
                //5. If the query string parameter of? _include = customer is in the URL, then the customer representation should be included in the response.
                if (_include == "customers")
                {

                    Dictionary<int, Order> orderDict = new Dictionary<int, Order>();

                    IEnumerable<Order> OrdandCust = Connection.Query<Order, Customer, Order>(
                    $@"SELECT o.Id,
                            o.CustomerId,
                            o.PaymentTypeId,
                            c.Id,
                            c.FirstName,
                            c.LastName
                        From [Order] o
                        JOIN Customer c ON o.CustomerId = c.Id
                         WHERE 1 = 1; 
                        ",
                    (newgeneratedOrder, generatedCustomer) =>
                    {
                        if (!orderDict.ContainsKey(newgeneratedOrder.Id))
                        {
                            orderDict[newgeneratedOrder.Id] = newgeneratedOrder;

                        }
                        orderDict[newgeneratedOrder.Id].customer = generatedCustomer;
                        return newgeneratedOrder;
                    }
                    );
                    return Ok(orderDict);
                }



            }

            using (IDbConnection conn = Connection)
                {

                    IEnumerable<Order> orders = await conn.QueryAsync<Order>(sql);
                    return Ok(orders);
                }
            
        }

    // GET -- Returns Specified Order (by Id given) api/paymentTypes/5
    [HttpGet("{id}", Name = "GetOrder")]
    public async Task<IActionResult> Get([FromRoute]int id, string _include)
    {
        string sql = $@"
            SELECT
                o.Id,
                o.CustomerId,
                o.PaymentTypeId
            FROM [Order] o
            WHERE o.Id = {id}
            ";

            //4. If the query string parameter of? _include = products is in the URL, then the list of products in the order should be returned.
            if (_include != null)
            {
                if (_include == "products")
                {
                    Dictionary<int, Order> products = new Dictionary<int, Order>();
                    IEnumerable<Order> OrdandOrdProdandProd = Connection.Query<Order, Product, Order>(
                        $@"SELECT o.Id,
                            o.CustomerId,
                            o.PaymentTypeId,
                            op.Id, 
                            op.OrderId,
                            op.ProductId,
                            p.Id,
                            p.Title
                        FROM Product p
                        JOIN OrderProduct op ON p.Id = op.ProductId
                        JOIN [Order] o ON o.Id = op.OrderId  
                         WHERE o.Id = {id}; 
                        ",

                        (generatedOrder, generatedProduct) =>
                        {
                            if (!products.ContainsKey(generatedOrder.Id))
                            {
                                products[generatedOrder.Id] = generatedOrder;
                            }

                            products[generatedOrder.Id].productList.Add(generatedProduct);
                            return generatedOrder;
                        }

                        );
                    return Ok(products);
                }
                //5. If the query string parameter of? _include = customer is in the URL, then the customer representation should be included in the response.
                if (_include == "customers")
                {

                    Dictionary<int, Order> orderDict = new Dictionary<int, Order>();

                    IEnumerable<Order> OrdandCust = Connection.Query<Order, Customer, Order>(
                    $@"SELECT o.Id,
                            o.CustomerId,
                            o.PaymentTypeId,
                            c.Id,
                            c.FirstName,
                            c.LastName
                        From [Order] o
                        JOIN Customer c ON o.CustomerId = c.Id
                         WHERE o.Id = {id}; 
                        ",
                    (newgeneratedOrder, generatedCustomer) =>
                    {
                        if (!orderDict.ContainsKey(newgeneratedOrder.Id))
                        {
                            orderDict[newgeneratedOrder.Id] = newgeneratedOrder;

                        }
                        orderDict[newgeneratedOrder.Id].customer = generatedCustomer;
                        return newgeneratedOrder;
                    }
                    );
                    return Ok(orderDict);
                }



            }

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Order> orders = await conn.QueryAsync<Order>(sql);
                return Ok(orders.Single());
            }
    }
 
        // POST api/paymentType
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order)
        {
            string sql = $@"INSERT INTO [Order] 
            (CustomerId)
            VALUES
            (
                '{order.CustomerId}'
                
                
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
            SET PaymentTypeId = '{order.PaymentTypeId}'
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

