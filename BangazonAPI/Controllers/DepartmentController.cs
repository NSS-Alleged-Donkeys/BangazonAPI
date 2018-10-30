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
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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

        [HttpGet]
        /* Below you need the strings of _include, _filter, and _gt (greater than)
        This is for the section where I am looking for employees in the departments */
        public async Task<IActionResult> Get(string _include, string _filter, int _gt)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = "SELECT * FROM Department";
                /* if statement including employees */
                if (_include == "employees")
                {
                    /* Creating a dictionary of departments titled "listOfDepartments */
                    Dictionary<int, Department> departmentList = new Dictionary<int, Department>();

                    /* SQL = this list. dpt = department and e = employee. Includes the properties from each class
                    The employee has a department ID and that is where the employee and department class join */
                    sql = @"SELECT
                            dpt.Id,
                            dpt.Name,
                            dpt.Budget,
                            e.Id,
                            e.FirstName,
                            e.LastName,
                            e.DepartmentId,                       
                            e.IsSupervisor
                            FROM Department dpt
                            JOIN Employee e ON dpt.Id = e.DepartmentId";
                    /* Creating a variable named "departmentEmployees. This is where the dapper magic happens
                    This is kind of like a foreach loop. It's looking for where the employee and department match and it
                    is adding that to the dictionary and building it up. */
                    var departmentEmployees = await conn.QueryAsync<Department, Employee, Department>(sql,
                        (department, employee) =>
                        {
                            if (!departmentList.ContainsKey(department.Id))
                            {
                                departmentList[department.Id] = department;
                            }
                            departmentList[department.Id].EmployeeList.Add(employee);
                            return department;
                        });
                    /* Returning the results of the departmentList */
                    return Ok(departmentList.Values);
                }

                /* For finding a budget. Steve asks for something that is greater than 300,000 but we intially did not populate the database with a 
                number greater than that. So I put 100,000 */
                if (_filter == "budget" && _gt >= 100000)
                    /* Looking into the SQL wfrom where the department budget is greater than or equal to 100,000 */
                {

                    sql = $@"SELECT * FROM Department WHERE Budget >= {_gt}";

                }
                /* The variable departmentBudget is using dapper to execute the SQL */
                var departmentBudget = await conn.QueryAsync<Department>(sql);
                /* Returning the values */
                return Ok(departmentBudget);
            }
        }

        // GET api/department/5
        [HttpGet("{id}", Name = "GetDepartment")]
        /* Getting a department by an ID */
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = $@"
            SELECT
                dpt.Id,
                dpt.Name,
                dpt.Budget
            FROM Department dpt
            WHERE dpt.Id = {id}
            ";
                var SingleDepartment = (await conn.QueryAsync<Department>(sql)).Single();
                return Ok(SingleDepartment);

            }
        }

        // POST api/department
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            string sql = $@"INSERT INTO Department 
            (Name, Budget)
            VALUES
            (
                '{department.Name}'
                ,'{department.Budget}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                department.Id = newId;
                return CreatedAtRoute("GetDepartment", new { id = newId }, department);
            }
        }

        // PUT api/department/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Department department)
        {
            string sql = $@"
            UPDATE Department
            SET Name = '{department.Name}',
                Budget = '{department.Budget}',
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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DepartmentExists(int id)
        {
            string sql = $"SELECT Id FROM Department WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Department>(sql).Count() > 0;
            }
        }
    }
}
