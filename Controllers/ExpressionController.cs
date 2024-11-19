using Microsoft.AspNetCore.Mvc;
using CodeExecuter.Models.Expression;

public class ExpressionController : Controller
{
    [HttpPost]
    public IActionResult Evaluate([FromBody] CodeRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("Invalid code");
        }

        string result = Expression.Evaluate(request.Code);
        return Ok(result);
    }
}

public class CodeRequest
{
    public required string Code { get; set; }
}