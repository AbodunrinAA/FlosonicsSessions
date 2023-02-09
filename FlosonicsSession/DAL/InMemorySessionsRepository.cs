using FlosonicsSession.Data;
using FlosonicsSession.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FlosonicsSession.DAL;

public class InMemorySessionsRepository : ISessionsRepository
{
    private readonly ApplicationDbContext _context;

    private readonly SemaphoreSlim _semaphore;

    public InMemorySessionsRepository(ApplicationDbContext applicationDbContext,
        SemaphoreSlim semaphore)
    {
        _context = applicationDbContext;
        _semaphore = semaphore;
    }
    
    /*Refactors the code to reduce the repetition of await _semaphore.WaitAsync()
     and _semaphore.Release()*/
    private async Task ExecuteQueryUnderSemaphore(Func<Task> action)
    {
        await _semaphore.WaitAsync();
        try
        {
            await action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int> AddSessionAsync(Session session)
    {
        Task<int> result = null;
        
        await ExecuteQueryUnderSemaphore(async () =>
        {
            await _context.Sessions.AddAsync(session); 
            result = _context.SaveChangesAsync();
        });

        return await result;
    }

    public async Task UpdateSession(Session session)
    {
        await ExecuteQueryUnderSemaphore(async () =>
        {
            _context.Entry(session).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        });
    }

    public async Task RemoveSessionAsync(Session session)
    {
        await ExecuteQueryUnderSemaphore(async () =>
        {
            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();
        });
    }

    public async Task<Session> GetSessionAsync(Guid id)
    {
        Session session = null;
        await ExecuteQueryUnderSemaphore(async () =>
        {
            session = await _context.Sessions.FindAsync(id);
            
        });
        return session;
    }
    
    public async Task<Session> GetSessionByETagAsync(Guid id)
    {
        Session session = null;
        await ExecuteQueryUnderSemaphore(async () =>
        {
            session = await _context.Sessions.FirstOrDefaultAsync(e=>e.ETag == id);
            
        });
        return session;
    }
    
    public async Task<Session> GetSessionByNameAsync(string name)
    {
        Session session = null;
        
        await ExecuteQueryUnderSemaphore(async () =>
        {
            session  = await _context.Sessions.Where(n=>n.Name.ToLower() == name.ToLower()).FirstOrDefaultAsync();
        });

        return session;
    }
    
    public async Task<Session> GetSessionByNameAndIdAsync(string name, Guid id)
    {
        Session session = null;
        
        await ExecuteQueryUnderSemaphore(async () =>
        {
            session  = await _context.Sessions.Where(n=>n.Name.ToLower() == name.ToLower() && n.Id == id).FirstOrDefaultAsync();
        });

        return session;
    }

    public async Task<IEnumerable<Session>> GetAllSessions()
    {
        IEnumerable<Session> sessions = null;
        await ExecuteQueryUnderSemaphore(async () =>
        {
            sessions = _context.Sessions.AsNoTracking().ToList();
        });
        return sessions;
    }

    public async Task<IEnumerable<Session>> GetSessionsWithPaginationAsync(string name, 
        string tag, int page, int pageSize)
        
        // This returns an IEnumerable<T> representation of the query and only loads data into memory as the data is enumerated.
         // This is known as "deferred execution." So, you can use AsEnumerable to switch to client-side evaluation,
        // but the data won't be loaded into memory until you start enumerating the data.
    {
        IEnumerable<Session> sessions = null;
        await ExecuteQueryUnderSemaphore(async () =>
        {
            sessions = _context.Sessions.AsEnumerable()
                    .Where(s => (name.IsNullOrEmpty() ||
                                 s.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                                && (tag.IsNullOrEmpty() || GetTags(s.Tags).Any(t => t.Contains(tag, StringComparison.InvariantCultureIgnoreCase))))
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
                
        });

        return sessions;
    }

    public async Task<TimeSpan> GetAverageSessionDurationAsync(DateTime startDate, DateTime endDate)
    {
        var averageDuration = new TimeSpan();
        await ExecuteQueryUnderSemaphore(async () =>
        {
            // Filter the sessions based on the provided date range
            var sessions = _context.Sessions
                .Where(s => s.DateAdded.Date >= startDate.Date && s.DateAdded.Date <= endDate.Date).AsEnumerable();

            // Calculate the average duration of the filtered sessions
            if(sessions.Count() != 0)
                    averageDuration =  TimeSpan.FromSeconds(sessions.Average(s => s.Duration.TotalSeconds));
        });
        // Return the average duration
        return averageDuration;
    }
    
    private IEnumerable<string> GetTags(string tags)
    {
        return tags.Split(',').AsEnumerable();
    }
    /*In most cases, you don't need to implement IDisposable in a data access class that uses
     Entity Framework Core (EF Core) in .NET 7, because EF Core implements it internally and automatically 
     releases the resources it uses when the context is no longer needed.*/
}