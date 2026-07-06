using ErpBackend.CrossCutting.Pagination;
using ErpBackend.CrossCutting.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ErpBackend.Api.Controllers;

/// <summary>
/// Base controller that standardizes how endpoints return the shared response envelopes.
/// Derive module controllers from this so every API stays consistent.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null)
        => Ok(ApiResponse<T>.Ok(data, message));

    protected ActionResult<PagedResponse<T>> PagedOk<T>(PagedResult<T> result, string? message = null)
        => Ok(PagedResponse<T>.From(result, message));

    protected ActionResult<CreatedResponse<TKey>> CreatedResponse<TKey>(TKey id, string? message = null)
        => StatusCode(StatusCodes.Status201Created, new CreatedResponse<TKey>(id, message));

    protected ActionResult<NoContentResponse> NoContentResponse(string? message = null)
        => Ok(new NoContentResponse(message));
}
