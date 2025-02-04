namespace IntraDotNet.EntityFrameworkCore.Interfaces;

public interface IAuditable: ICreateAuditable, IUpdateAuditable, ISoftDeleteAuditable {
}