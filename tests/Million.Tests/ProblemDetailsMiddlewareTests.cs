using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Million.Web.Middlewares;
using System.Text.Json;

namespace Million.Tests;

public class ProblemDetailsMiddlewareTests
{
    [Test]
    public async Task Unhandled_exception_results_in_problem_details_500()
    {
        var context = new DefaultHttpContext();
        var middleware = new ProblemDetailsMiddleware(_ => throw new Exception("boom"), NullLogger<ProblemDetailsMiddleware>.Instance);
        await middleware.InvokeAsync(context);
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}

