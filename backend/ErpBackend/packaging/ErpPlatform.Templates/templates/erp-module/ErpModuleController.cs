using ErpBackend.CrossCutting.Responses;
using ErpBackend.CrossCutting.Security;
using Microsoft.AspNetCore.Mvc;

namespace ErpApp.Modules;

/// <summary>
/// Skeleton controller wired to the ERP Platform (responses + permission authorization).
/// Implement the remaining CRUD, or generate the full module from a manifest with `erpgen`.
/// </summary>
[ApiController]
[Route("api/erp-resource")]
[Produces("application/json")]
public class ErpModuleController : ControllerBase
{
    [HttpGet]
    [HasPermission("erp-prefix.view")]
    public ActionResult<ApiResponse<object>> Get()
        => Ok(ApiResponse<object>.Ok(null, "TODO: implement. For full CRUD from a manifest, run `erpgen module --manifest ...`."));
}
