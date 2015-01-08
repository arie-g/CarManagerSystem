using System.Collections.Generic;
using System.Web.Http;

namespace CarManagerWebApplication.Controllers
{
    public class StatsController : ApiController
    {
        // GET api/<controller>
        [Authorize]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        [Authorize]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [Authorize]

        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [Authorize]

        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [Authorize]
        public void Delete(int id)
        {
        }
    }
}