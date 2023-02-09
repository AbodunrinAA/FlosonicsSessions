using FlosonicsSession.Models;

namespace FlosonicsSession.DAL;

public interface ISessionsRepository
{
    Task<Session> GetSessionByNameAsync(string name);
    
    public Task<int> AddSessionAsync(Session session);

    public Task UpdateSession(Session session);

    public Task RemoveSessionAsync(Session session);

    public Task<Session> GetSessionAsync(Guid id);

    Task<Session> GetSessionByETagAsync(Guid id);

    public Task<IEnumerable<Session>> GetAllSessions();

    Task<IEnumerable<Session>> GetSessionsWithPaginationAsync(string name,
        string tag, int page, int pageSize);

    Task<TimeSpan> GetAverageSessionDurationAsync(DateTime startDate, DateTime endDate);
}