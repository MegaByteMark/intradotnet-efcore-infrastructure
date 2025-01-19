public interface ISoftDeleteAuditable {
    public DateTimeOffset DeletedOn {get;set;}
    public string DeletedOn {get;set;}

}