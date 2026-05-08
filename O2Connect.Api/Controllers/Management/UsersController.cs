using Microsoft.AspNetCore.Mvc;

namespace O2Connect.Api.Controllers.Management;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok("GET /users");
    }

    [HttpGet("{id}")]
    public IActionResult GetUser(string id)
    {
        return Ok($"GET /users/{id}");
    }

    [HttpPost]
    public IActionResult CreateUser()
    {
        return Ok("POST /users");
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(string id)
    {
        return Ok($"PUT /users/{id}");
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(string id)
    {
        return Ok($"DELETE /users/{id}");
    }
}
