using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly string connectionString = "Server=DUNG-KIRIN;Database=DatingApp;User Id=sa;Password=cntt;MultipleActiveResultSets=true;";
        private SqlConnection sqlConnection;

        public async Task<JArray> Get()
        {
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();
                var dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@country", "uk");
                var data= await sqlConnection.QueryAsync<User>(
                      "GetUserByCountry",
                      dynamicParameters,
                      commandType: CommandType.StoredProcedure);

                return JArray.FromObject(data);
            }

        }

    }
}