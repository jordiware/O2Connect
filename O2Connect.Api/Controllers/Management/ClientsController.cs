using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("clients")]
public class ClientsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetClients()
    {
        return Ok("GET /clients");
    }

    [HttpPost]
    public IActionResult CreateClient()
    {
        return Ok("POST /clients");
    }

    [HttpPut("{id}")]
    public IActionResult UpdateClient(string id)
    {
        return Ok($"PUT /clients/{id}");
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteClient(string id)
    {
        return Ok($"DELETE /clients/{id}");
    }
}
