namespace IntraDotNet.EntityFrameworkCore.Infrastructure.Interfaces;

public interface IAuditable: ICreateAuditable, IUpdateAuditable, ISoftDeleteAuditable {
}