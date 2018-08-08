using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ExtendedLoopAlarms.Utils;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExtendedLoopAlarms
{
    [Route("api/[controller]")]

    public class MetricsController : Controller
    {
        public static string NS = typeof(ExtendedLoopAlarms.MetricsController).Namespace;

        private List<DeviceStatus> fetchNightscoutDeviceStatus(string baseurl){

            if (String.IsNullOrWhiteSpace(baseurl)) {
                throw new InvalidOperationException("nightscout site was invalid");
            }
            var url = $"{baseurl}/api/v1/devicestatus.json?find[device][$regex]=loop://&count=1";
            var client = new WebClient();
            //Logger.LogInfo($"Calling url {url}");

            var contents = client.DownloadString(url);
            return JsonConvert.DeserializeObject<List<DeviceStatus>>(contents);
        }

        [HttpGet("/api")]
        public string GetIndex(){
            //https://localhost:5001/api

            return "/api/Index should show metainformation, however, this is currently not supported. See /swagger instead";
        }
        // GET: api/metrics
        [HttpGet]
        public string Get()
        {
            return "this would show all metrics, however, this is currently not supported";


            //https://localhost:5001/api/metrics
            //return new string[] { "value1", "value2" };
        }

        //https://localhost:5001/api/metrics/foo
        //https://localhost:5001/api/metrics/loophasrun
        //[HttpGet("{metrictype}")]
        /*[HttpGet("loophasrunthelastxminute/")]
        public string Get()
        {
            return $"Got metrictype loophasrunthelastxminute  yey, but you need to specify a minago parameter!111";

        }*/

        /// <summary>
        /// A metric that returns "true" or "false" (or error message) based on if your nightscout site has loop events registered the last {minago} minutes
        /// </summary>
        /// <param name="minAgoThreshold">minutes to lookback for loop events</param>
        /// <returns>string</returns>
        [HttpGet("loophascrashed/{minAgoThreshold}")]
        public string Get(int minAgoThreshold=15)
        {
            if(minAgoThreshold < 0 ) {
                return JsonConvert.SerializeObject(new { message = "minago invalid" });
                //return new JsonResult(new { message = "minago invalid" });
            }
            // NightscoutPebble nsglucose = null;
            List<DeviceStatus> device = null;
            Exception lasterror = null;
            var i = 0;
            do
            {
                
                //var n = i + 1;
                try
                {
                    //Logger.LogInfo($"Attempt {n} to fetch glucose from {Config.NsHost}");
                    device = this.fetchNightscoutDeviceStatus(Config.Instance.NsHost);
                    //Logger.LogInfo($"Got {device.Count} entries from nightscout");
                    lasterror = null;
                }
                catch (Exception err)
                {
                    lasterror = err;
                }
                Console.WriteLine($"attempt {i + 1}: {lasterror?.Message}");
                //will try this a max of three times
            } while (i++ < 2 && lasterror != null);

            if (lasterror != null)
            {
                return JsonConvert.SerializeObject(new { message = lasterror.Message });
            }
            if (device == null || device.FirstOrDefault() == null) {
                return JsonConvert.SerializeObject(new { message = "Could not find any loop related events at your nightscout" });
            }
            // assumed lastloop is in utc
            var lastLoop = device.FirstOrDefault().created_at.ToUniversalTime();

            var now = DateTime.UtcNow;
            var diff = now - lastLoop;
            var loophascrashed = diff.TotalSeconds >= (minAgoThreshold * 60);
            return JsonConvert.SerializeObject(loophascrashed );


        }

        /*// POST https://localhost:5001/api/metrics
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT https://localhost:5001/api/metrics/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE https://localhost:5001/api/metrics/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        */
    }
}
