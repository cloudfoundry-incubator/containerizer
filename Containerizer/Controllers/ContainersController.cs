﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Web.Http;
using Containerizer.Facades;
using Newtonsoft.Json;
using Containerizer.Services.Interfaces;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Web.WebSockets;
using Containerizer.Services.Implementations;

namespace Containerizer.Controllers
{
    public class CreateResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class ContainersController : ApiController
    {
        private readonly ICreateContainerService createContainerService;
        private readonly IStreamOutService streamOutService;
        private IStreamInService streamInService;

        public ContainersController(
            ICreateContainerService createContainerService,
            IStreamInService streamInService,
            IStreamOutService streamOutService
        )
        {
            this.createContainerService = createContainerService;
            this.streamOutService = streamOutService;
            this.streamInService = streamInService;
        }

        [Route("api/containers")]
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                var id = await createContainerService.CreateContainer();
                return Json(new CreateResponse { Id = id });
            }
            catch (CouldNotCreateContainerException ex)
            {
                return this.InternalServerError(ex);
            }
        }

        [Route("api/containers/{id}/files")]
        [HttpGet]
        public Task<HttpResponseMessage> StreamOut(string id, string source)
        {
            var outStream = streamOutService.StreamOutFile(id, source);
            var response = Request.CreateResponse();
            response.Content = new StreamContent(outStream);
            return Task.FromResult(response);
        }

        [Route("api/containers/{id}/run")]
        [HttpGet]
        public Task<HttpResponseMessage> Run(string id)
        {
            HttpContext.Current.AcceptWebSocketRequest(new ContainerProcessHandler(new ProcessFacade()));
            var response = Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            return Task.FromResult(response);
        }

        [Route("api/containers/{id}/files")]
        [HttpPut]
        public async Task<HttpResponseMessage> StreamIn(string id, string destination)
        {
            var stream = await Request.Content.ReadAsStreamAsync();
            streamInService.StreamInFile(stream, id, destination);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
