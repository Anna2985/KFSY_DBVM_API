using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Configuration;
using Basic;
using HIS_DB_Lib;
namespace DB2VM
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
      
        // GET api/values
        [HttpGet]
        public string Get()
        {
       

        
        
            return $"DB2 Connecting sucess!";


        }
        

    }
}
