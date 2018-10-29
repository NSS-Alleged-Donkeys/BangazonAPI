﻿using System;
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
        private object conn;

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
        public async Task<IActionResult> Get(string _include, string _filter, int _gt)
        {
            using (IDbConnection conn = Connection)
            {
                string sql = "SELECT * FROM Department";

                if (_include == "employees")
                {
                    Dictionary<int, Department> listOfDepartments = new Dictionary<int, Department>();

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

                    var departmentEmployees = await conn.QueryAsync<Department, Employee, Department>(sql,
                        (department, employee) =>
                        {
                            if (!listOfDepartments.ContainsKey(department.Id))
                            {
                                listOfDepartments[department.Id] = department;
                            }
                            listOfDepartments[department.Id].EmployeeList.Add(employee);
                            return department;
                        });
                    return Ok(listOfDepartments.Values);
                }

                if (_filter == "budget" && _gt >= 100000)
                {

                    sql = $@"SELECT * FROM Department WHERE Budget >= {_gt}";

                }

                var departmentBudget = await conn.QueryAsync<Department>(sql);
                return Ok(departmentBudget);
            }
        }

        // GET api/department/5
        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                dpt.Id,
                dpt.Name,
                dpt.Budget
            FROM Department dpt
            WHERE dpt.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Department> departments = await conn.QueryAsync<Department>(sql);
                return Ok(departments);
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
