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
        public async Task<IActionResult> Get(string q)
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

            using (IDbConnection conn = Connection)
            {

                IEnumerable<TrainingProgram> trainingprogram = await conn.QueryAsync<TrainingProgram>(sql);
                return Ok(trainingprogram);
            }
        }

        //   GET /TrainingProgram?_include=employees
        //   GET /TrainingProgram?completed=false
        //This GET method will retrieve the information from the database for TrainingProgram
        //You can also include employees in the retrieval if you want to see which ones are signed up for the training programs
        //You can also filter by which training programs have been completed
        [HttpGet]
        public async Task<IActionResult> Get(string _include, string completed)
        {

            using (IDbConnection conn = Connection)
            {

                string sql = "Select * from TrainingProgram LEFT JOIN EmployeeTraining ON TrainingProgram.TrainingProgramId = EmployeeTraining.EmployeeTrainingId";

                if (_include != null && _include.Contains("employee"))
                {
                    sql = $"Select * FROM TrainingProgram " +
                        $"LEFT JOIN EmployeeTraining ON TrainingProgram.TrainingProgramId = EmployeeTraining.TrainingProgramId " +
                        $"LEFT JOIN Employee ON EmployeeTraining.EmployeeId = Employee.EmployeeId ";

                    Dictionary<int, TrainingProgram> report = new Dictionary<int, TrainingProgram>();
                    var fullTrainingProgram = await conn.QueryAsync<TrainingProgram, Employee, TrainingProgram>(
                    sql, (trainingProgram, employee) =>
                    {
                        // Does the Dictionary already have the key of the Employee?
                        if (!report.ContainsKey(trainingProgram.TrainingProgramId))
                        {
                            // Create the entry in the dictionary
                            report[trainingProgram.TrainingProgramId] = trainingProgram;
                        }

                        // Add the Employees to the current TrainingProgram entry in Dictionary
                        report[trainingProgram.TrainingProgramId].Employees.Add(employee);
                        return trainingProgram;
                    }, splitOn: "TrainingProgramId"
                        );
                    return Ok(report.Values);
                }
                //Checking to see if the Training Program is completed by adding an additional "WHERE" to our sql statement to filter out dates that were in the past from today
                if (completed == "false")
                {
                    DateTime current = DateTime.Today;
                    sql += $" WHERE TrainingProgram.endDate >= '{current}'";
                }
                var trainingPrograms = await conn.QueryAsync<TrainingProgram>(sql);
                return Ok(trainingPrograms);
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
            string sql = $@"DELETE FROM TrainingProgram WHERE Id = {id}";

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