namespace Calendar.Application.Abstraction.Repositories;

public interface IUserLookupRepository
{
    Task<IList<Guid>> GetUserIdsByEmailsAsync(IList<string> emails, CancellationToken ct = default);
}