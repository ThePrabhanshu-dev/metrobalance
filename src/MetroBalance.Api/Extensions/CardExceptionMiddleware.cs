using MetroBalance.Core.Contracts;
using MetroBalance.Core.Exceptions;

namespace MetroBalance.Api.Extensions;

/// <summary>
/// Translates domain exceptions into the standard <see cref="ApiResponse{T}"/> envelope
/// with meaningful HTTP status codes.
/// </summary>
public sealed class CardExceptionMiddleware(RequestDelegate next, ILogger<CardExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (InvalidCardNumberException ex)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, "INVALID_CARD_NUMBER", ex.Message);
        }
        catch (CardNotFoundException ex)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, "CARD_NOT_FOUND", ex.Message);
        }
        catch (CardNotActiveException ex)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, "CARD_NOT_ACTIVE", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error while processing {Path}", context.Request.Path);
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "UNEXPECTED_ERROR",
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var payload = ApiResponse<object>.Fail(errorCode, message);
        await context.Response.WriteAsJsonAsync(payload);
    }
}
