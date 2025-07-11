using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Core.Application.Contracts.HandlerExchanges.Post.Queries;
using Core.Application.Contracts.HandlerExchanges.Post.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Framework.Permissions;

namespace Web.Api.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize]
    public class PostController : BaseApiController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.Posts.View)]
        [ProducesResponseType(typeof(IEnumerable<GetAllPostQueryResponse>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<IEnumerable<GetAllPostQueryResponse>>> GetPosts()
        {
            var products = await Mediator.Send(new GetAllPostQuery());
            return Ok(products);
        }

        [HttpPost]
        [Authorize(Policy = Permissions.Posts.Create)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<IActionResult> CreatePost([FromBody] AddPostCommand command)
        {
            var result = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetPosts), new { }, result); // adjust as needed
        }

        
    }
}
