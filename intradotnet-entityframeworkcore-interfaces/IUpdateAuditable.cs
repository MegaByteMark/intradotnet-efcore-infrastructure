public interface IUpdateAuditable
{
    public DateTimeOffset? LastUpdateOn { get; set; }
    public string? LastUpdateBy { get; set; }
}