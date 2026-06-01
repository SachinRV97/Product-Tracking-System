using Microsoft.EntityFrameworkCore;
using ProductTrackingSystem.Data;

namespace ProductTrackingSystem.Services;

public interface ITagNumberService
{
    Task<string> GenerateAsync(int companyId);
}

public class TagNumberService : ITagNumberService
{
    private readonly ApplicationDbContext _context;
    public TagNumberService(ApplicationDbContext context) => _context = context;

    public async Task<string> GenerateAsync(int companyId)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Products.CountAsync(x => x.CompanyId == companyId && x.CreatedAtUtc.Year == year);
        return $"TAG-{companyId:000}-{year}-{count + 1:00000}";
    }
}
