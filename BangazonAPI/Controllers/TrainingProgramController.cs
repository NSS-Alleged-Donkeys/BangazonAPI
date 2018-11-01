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
    public class TrainingProgramController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramController(IConfiguration config)
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

        // GET api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get(string q, string completed)
        {
            string sql = @"
            SELECT 
                tp.Id,
                tp.StartDate,
                tp.EndDate,
                tp.MaxAttendees
            FROM TrainingProgram tp
            WHERE 1=1";

            if (q != null)
            {
                string isQ = $@"
                    AND tp.StartDate LIKE '%{q}%'
                    OR tp.EndDate LIKE '%{q}%'
                ";
                sql = $"{sql} {isQ}";
            }

            if(completed == "false")
            {  
                sql = sql + "AND StartDate > CONVERT(DATETIME,{fn CURDATE()});";
            }

            using (IDbConnection conn = Connection)
            {

                IEnumerable<TrainingProgram> trainingprogram = await conn.QueryAsync<TrainingProgram>(sql);
                return Ok(trainingprogram);
            }
        }

        // GET trainingprogram/2
        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (IDbConnection conn = Connection)
            {
                /*Joining the EmployeeTraining table w/ the TrainingProgram/Employee table */
                string sql = $"Select * FROM TrainingProgram " +
                        $"LEFT JOIN EmployeeTraining ON TrainingProgram.Id = EmployeeTraining.Id " +
                        $"LEFT JOIN Employee ON EmployeeTraining.Id = Employee.Id " +
                        $"WHERE TrainingProgram.Id = {id}";
                /* Adding Employees to the Training Programs */
                Dictionary<int, TrainingProgram> listOfPrograms = new Dictionary<int, TrainingProgram>();
                var SingleTrainingProgram = (await conn.QueryAsync<TrainingProgram, Employee, TrainingProgram>(
                sql, (TrainingProgram, employee) =>
                {
                    if (!listOfPrograms.ContainsKey(TrainingProgram.Id))
                    {
                        listOfPrograms[TrainingProgram.Id] = TrainingProgram;
                    }
                    listOfPrograms[TrainingProgram.Id].EmployeeList.Add(employee);
                    return TrainingProgram;
                }, splitOn: "Id"
                    )).Single();
                return Ok(listOfPrograms.Values);
            }
        }


        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingprogram)
        {
            string sql = $@"INSERT INTO TrainingProgram 
            (StartDate, EndDate, MaxAttendees)
            VALUES
            (
                '{trainingprogram.StartDate}',
                '{trainingprogram.EndDate}',
                '{trainingprogram.MaxAttendees}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                trainingprogram.Id = newId;
                return CreatedAtRoute("GetTrainingProgram", new { id = newId }, trainingprogram);
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TrainingProgram trainingprogram)
        {
            string sql = $@"
            UPDATE TrainingProgram
            SET StartDate = '{trainingprogram.StartDate}',
                EndDate = '{trainingprogram.EndDate}',
                MaxAttendees = '{trainingprogram.MaxAttendees}'
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
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            /* The CONVERT() function is a general function that converts an expression of one data type to another.
             * In this case it converts DATETIME into CURDATE (CurrentDate).*/
            string sql = $@"DELETE FROM TrainingProgram WHERE Id = {id}";
            sql = sql + "AND StartDate > CONVERT(DATETIME,{fn CURDATE()});";

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

        private bool TrainingProgramExists(int id)
        {
            string sql = $"SELECT Id FROM TrainingProgram WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<TrainingProgram>(sql).Count() > 0;
            }
        }
    }
}