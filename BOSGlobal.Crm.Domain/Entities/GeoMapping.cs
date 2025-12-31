using System.ComponentModel.DataAnnotations;

namespace BOSGlobal.Crm.Domain.Entities;

public class GeoMapping
{
    public Guid Id { get; set; }
    [MaxLength(450)]
    public string? UserId { get; set; }
    [MaxLength(64)]
    public string LocationType { get; set; } = "Home"; // Home, Office, Customer
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int RadiusMeters { get; set; } = 150;
    public DateTime CreatedUtc { get; set; }
}
