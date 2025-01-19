public interface IAuditable: ICreateAuditable, IUpdateAuditable, ISoftDeleteAuditable {
    public DateTimeOffset? CreatedOn { get; set;}
    public string? CreatedBy {get; set;}
    public DateTimeOffset? LastUpdateOn {get;set;}
    public string? LastUpdateBy {get;set;}
    public DateTimeOffset? DeletedOn {get;set;}
    public string? DeletedOn {get;set;}
}