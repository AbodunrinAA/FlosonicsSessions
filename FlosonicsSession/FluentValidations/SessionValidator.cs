using FlosonicsSession.DAL;
using FlosonicsSession.DTOs;
using FlosonicsSession.Helpers;
using FluentValidation;

namespace FlosonicsSession.FluentValidations;

public interface ISessionValidator : IValidator<ISessionDto>
{
    ISessionValidator setIsAdd(bool isAdd);
}
public class SessionValidator : AbstractValidator<ISessionDto>, ISessionValidator
{
    private bool isAdd;
    
    private readonly ISessionsRepository _sessionsRepository;

    public SessionValidator(ISessionsRepository sessionsRepository)
    {
        _sessionsRepository = sessionsRepository;

        RuleFor(s => s.Tags).Must(TagsMustBeUnique).WithMessage("All values must be unique within the collection");
        RuleFor(s => s.Duration).NotEmpty().WithMessage("Duration is required").NotNull()
            .WithMessage("Duration is required").Must(x => TimeSpan.Parse(x) <= TimeSpan.FromHours(1))
            .WithMessage("Cannot exceed 1 hour");

        RuleFor(s => s.Name).NotEmpty().WithMessage("Name is required").NotNull().WithMessage("Name cannot be null")
                .Length(1, 50).WithMessage("Name should be 1 - 50 characters in length").Must(NameMustBeUnique)
                .WithMessage(MagicStrings.NameExists);
        
    }
    
    public ISessionValidator setIsAdd(bool isAdd)
    {
        this.isAdd = isAdd;
        return this;
    }
    
    private bool TagsMustBeUnique(ISessionDto session, string tags)
    {
        var tags_ = tags.Split(',');
        return tags_.Distinct().Count() == tags_.Length;
    }

    private bool NameMustBeUnique(ISessionDto session, string name)
    {
        // Implementation to check if the name is unique among all other sessions
        // Two options are provided here 
        // 1. Look before you leap (LBYL): which is exactly what we are doing here. This checks if the name
        // is already exist and return true false if it is.
        // 2. It is easier to ask forgiveness than permission: Since name has been configured to be unique in the
        // database through ApplicationDbContext OnModelCreating, even without this method, an attempt to insert a
        // name that's already in the database will fail after all.
        
        if (isAdd)
        {
            return  _sessionsRepository.GetSessionByNameAsync(name)
                .GetAwaiter().GetResult() != default(ISession)
                ? false
                : true;
        }
        return true;
    }
}

public class SessionValidatorFactory : ISessionValidatorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SessionValidatorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ISessionValidator Create(bool isAdd)
    {
        return _serviceProvider.GetService<ISessionValidator>().setIsAdd(isAdd);
        
    }
}

public interface ISessionValidatorFactory
{
    ISessionValidator Create(bool isAdd);
}