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

    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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

        // GET api/Computer?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string sql = @"
            SELECT
                c.Id,
                c.PurchaseDate,
                c.DecomissionDate       
            FROM Computer c
            WHERE 1=1
            ";
            
            using (IDbConnection conn = Connection)
            {

                IEnumerable<Computer> Computers = await conn.QueryAsync<Computer>(sql);
                return Ok(Computers);
            }
        }

        // GET api/Computer/1
        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                c.Id,
                c.PurchaseDate,
                c.DecomissionDate
            FROM Computer c
            WHERE c.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Computer> computers = await conn.QueryAsync<Computer>(sql);
                return Ok(computers);
            }
        }

        // POST api/Computer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            string sql = $@"INSERT INTO Computer 
            (PurchaseDate, DecomissionDate)
            VALUES
            (
                '{computer.PurchaseDate}',
                '{computer.DecomissionDate}'
             
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                computer.Id = newId;
                return CreatedAtRoute("GetComputer", new { id = newId }, computer);
            }
        }

        // PUT api/Computer/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Computer computer)
        {
            string sql = $@"
            UPDATE Computer
            SET PurchaseDate = '{computer.PurchaseDate}', 
                DecomissionDate = '{computer.DecomissionDate}'
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
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/Computer/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Computer WHERE Id = {id}";

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

        private bool ComputerExists(int id)
        {
            string sql = $"SELECT Id FROM Computer WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Computer>(sql).Count() > 0;
            }
        }
    }
}



