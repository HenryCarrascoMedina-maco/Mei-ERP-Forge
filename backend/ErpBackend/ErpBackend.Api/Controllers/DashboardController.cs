using ErpBackend.CrossCutting.Constants;
using ErpBackend.CrossCutting.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErpBackend.Api.Controllers;

public record KpiDto(string Key, string Label, double Value, string Trend, double Percentage, string Status);
public record StatusSummaryDto(string Status, int Count);
public record RecentItemDto(string Title, string Subtitle, string Status, DateTime Date);

public record DashboardStatsDto(
    IReadOnlyList<KpiDto> Kpis,
    IReadOnlyList<StatusSummaryDto> StatusSummary,
    IReadOnlyList<RecentItemDto> Recent);

/// <summary>
/// Application home dashboard. Returns aggregated KPIs, a status summary and recent records through
/// the shared <see cref="ApiResponse{T}"/> envelope (consumed by the KPI/card components). Replace
/// the placeholder aggregates with queries over your own modules.
/// </summary>
[Route("api/dashboard")]
[Authorize]
public class DashboardController : BaseApiController
{
    [HttpGet("stats")]
    public ActionResult<ApiResponse<DashboardStatsDto>> GetStats()
    {
        var stats = new DashboardStatsDto(
            Kpis:
            [
                new("users", "Total users", 128, "up", 8.4, "primary"),
                new("active", "Active users", 112, "up", 3.1, "success"),
                new("categories", "Categories", 12, "neutral", 0, "info"),
                new("pending", "Pending tasks", 7, "down", 12.0, "warning"),
            ],
            StatusSummary:
            [
                new("Active", 112),
                new("Inactive", 16),
                new("Pending", 7),
                new("Archived", 4),
            ],
            Recent:
            [
                new("Ada Lovelace", "Administrator", "Active", DateTime.UtcNow.AddHours(-2)),
                new("Electronics", "Catalog category", "Active", DateTime.UtcNow.AddHours(-5)),
                new("Alan Turing", "Manager", "Pending", DateTime.UtcNow.AddDays(-1)),
                new("Clothing", "Catalog category", "Inactive", DateTime.UtcNow.AddDays(-2)),
            ]);

        return OkResponse(stats, SuccessMessages.Retrieved);
    }
}
