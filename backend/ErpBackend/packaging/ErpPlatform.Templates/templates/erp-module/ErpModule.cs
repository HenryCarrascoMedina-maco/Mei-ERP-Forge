using ErpBackend.CrossCutting.Common;

namespace ErpApp.Modules;

/// <summary>
/// Skeleton entity. Add your fields below, then implement CRUD in the controller.
/// For full manifest-driven generation (entity + DTOs + validation + controller + repo),
/// use:  erpgen module --manifest YourModule.module.json --project &lt;projectDir&gt;
/// </summary>
public class ErpModule : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    // TODO: add your fields.
}
