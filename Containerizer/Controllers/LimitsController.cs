﻿#region

using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Containerizer.Services.Interfaces;
using Newtonsoft.Json;
using IronFrame;
using System;

#endregion

namespace Containerizer.Controllers
{

    public class MemoryLimits
    {
        [JsonProperty("limit_in_bytes")]
        public ulong LimitInBytes { get; set; }
    }

    public class LimitsController : ApiController
    {
        private readonly IContainerService containerService;

        public LimitsController(IContainerService containerService)
        {
            this.containerService = containerService;
        }

        [Route("api/containers/{handle}/memory_limit")]
        [HttpPost]
        public IHttpActionResult LimitMemory(string handle, MemoryLimits limits)
        {
            var container = containerService.GetContainerByHandle(handle);
            if (container == null)
            {
                return NotFound();
            }

            container.LimitMemory(limits.LimitInBytes);
            return Ok();
        }

        [Route("api/containers/{handle}/memory_limit")]
        [HttpGet]
        public IHttpActionResult CurrentMemoryLimit(string handle)
        {
            var container = containerService.GetContainerByHandle(handle);
            if (container == null)
            {
                return NotFound();
            }
            var limit = container.CurrentMemoryLimit();
            return Json(new MemoryLimits { LimitInBytes = limit });
        }
    }
}
