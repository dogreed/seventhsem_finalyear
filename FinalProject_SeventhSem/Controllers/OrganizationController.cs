using FinalProject_SeventhSem.Application.Features.Admin.Queries.GetOrganizationDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinalProject_SeventhSem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ApiController
    {
        private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


        [HttpGet("dashboard")]
        [Authorize(Roles = "Organization")]
        [ProducesResponseType(typeof(OrganizationDashboardResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard(CancellationToken ct)
       => Ok(await Sender.Send(new GetOrganizationDashboardQuery(), ct));
    }
}
