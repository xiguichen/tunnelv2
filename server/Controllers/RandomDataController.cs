using Microsoft.AspNetCore.Mvc;

namespace server.Controllers;

[ApiController]
[Route("[controller]")]
public class RandomDataController : ControllerBase
{
    [HttpGet("{byteCount}")]
    public IActionResult GetRandomData(int byteCount)
    {
        // Generate random bytes
        byte[] randomBytes = new byte[byteCount];
        Random random = new Random();
        random.NextBytes(randomBytes);

        // Return the random bytes in the response
        return File(randomBytes, "application/octet-stream");
    }
}
