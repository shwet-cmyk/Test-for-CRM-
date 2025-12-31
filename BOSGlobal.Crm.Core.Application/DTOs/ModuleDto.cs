namespace BOSGlobal.Crm.Application.DTOs;

public class ModuleDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
