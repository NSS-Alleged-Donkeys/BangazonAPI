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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public EmployeeController(IConfiguration config)
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

        Dictionary<int, Employee> CulminatedEmployee = new Dictionary<int, Employee>();

        // GET: api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT
                e.Id,
                e.FirstName,
                e.LastName,
                e.DepartmentId,
                e.IsSuperVisor,
                d.Id,
	            d.Name,
	            d.Budget,
	            c.Id,
	            c.PurchaseDate,
	            c.DecomissionDate
            FROM Employee e
            JOIN Department d ON e.DepartmentId = d.Id
            JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
            JOIN Computer c ON c.Id = ce.ComputerId
            WHERE 1=1
            ";

            if (q != null)
            {
                string isQ = $@"
                    AND e.FirstName LIKE '%{q}%'
                    OR e.LastName LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Employee> employees = await conn.QueryAsync<Employee, Department, Computer, Employee>(sql, (employee, department, computer) => {
                    if (!CulminatedEmployee.ContainsKey(employee.Id))
                    {
                        employee.Department = department.Name;
                        employee.Computers = new List<Computer>();
                        CulminatedEmployee[employee.Id] = employee;
                    }
                    CulminatedEmployee[employee.Id].Computers.Add(computer);
                    return employee;
                });
                return Ok(employees);
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}", Name = "ReturnEmployeeObject")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            string sql = $@"
            SELECT
                e.Id,
                e.FirstName,
                e.LastName,
                e.DepartmentId,
                e.IsSuperVisor,
                d.Id,
	            d.Name,
	            d.Budget,
	            c.Id,
	            c.PurchaseDate,
	            c.DecomissionDate
            FROM Employee e
            JOIN Department d ON e.DepartmentId = d.Id
            JOIN ComputerEmployee ce ON e.Id = ce.EmployeeId
            JOIN Computer c ON c.Id = ce.ComputerId
            WHERE e.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Employee> employees = await conn.QueryAsync<Employee, Department, Computer, Employee>(sql, (employee, department, computer) => {
                    if (!CulminatedEmployee.ContainsKey(employee.Id))
                    {
                        employee.Department = department.Name;
                        employee.Computers = new List<Computer>();
                        CulminatedEmployee[employee.Id] = employee;
                    }
                    CulminatedEmployee[employee.Id].Computers.Add(computer);
                    return employee;
                });
                return Ok(employees.Single());
            }
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            string sql = $@"INSERT INTO Employee 
            (FirstName, LastName, DepartmentId, IsSuperVisor)
            VALUES
            (
                '{employee.FirstName}',
                '{employee.LastName}',
                '{employee.DepartmentId}',
                '{employee.IsSuperVisor}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                employee.Id = newId;
                return CreatedAtRoute("ReturnEmployeeObject", new { id = newId }, employee);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Employee employee)
        {
            string sql = $@"
            UPDATE Employee
            SET FirstName = '{employee.FirstName}',
                LastName = '{employee.LastName}',
                DepartmentId = '{employee.DepartmentId}',
                IsSuperVisor = '{employee.IsSuperVisor}'
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
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool EmployeeExists(int id)
        {
            string sql = $"SELECT Id FROM Employee WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Employee>(sql).Count() > 0;
            }
        }
    }
}
