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
        public async Task<IActionResult> Get([FromRoute]int id, [FromRoute]string _include)
        {
            string sql = $@"
            SELECT
                c.Id,
                c.FirstName,
                c.LastName
            FROM Customer c
            WHERE c.Id = {id}
            ";

            if (_include != null)
            {
                if (_include == "products")
                {
                    Dictionary<string, List<Employee>> report = new Dictionary<string, List<Employee>>();

                    IEnumerable<Department> deptsAndEmps = db.Query<Department, Employee, Department>(
                        @"
                    SELECT d.Id,
                        d.DeptName,
                        e.Id,
                        e.FirstName,
                        e.LastName,
                        e.DepartmentId
                    FROM Department d
                    JOIN Employee e ON e.DepartmentId = d.Id
                ",
                        (generatedDepartment, generatedEmployee) => {
                            if (!report.ContainsKey(generatedDepartment.DeptName))
                            {
                                report[generatedDepartment.DeptName] = new List<Employee>();
                            }
                            report[generatedDepartment.DeptName].Add(generatedEmployee);

                            return generatedDepartment;
                        }
                    );

                    string isQ = $@"
                    JOIN Product p ON p.CustomerId = c.Id";
                    sql = $"{sql} {isQ}";
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