using AutoMapper;
using FlosonicsSession.DAL;
using Microsoft.AspNetCore.Mvc;
using FlosonicsSession.DTOs;
using FlosonicsSession.FluentValidations;
using FlosonicsSession.Models;
using FluentValidation.AspNetCore;
using FlosonicsSession.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using static FlosonicsSession.Helpers.MagicStrings;

namespace FlosonicsSession.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ISessionValidatorFactory _validator;
        private readonly ISessionsRepository _iSessionsRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SessionController> _logger;

        public SessionController(ISessionValidatorFactory validator,
            ISessionsRepository iSessionsRepository, ILogger<SessionController> logger, IMapper mapper)
        {
            
            _validator = validator;
            _iSessionsRepository = iSessionsRepository;
            _logger = logger;
            _mapper = mapper;
        }

        
        // GET: api/Session/get-session/5
        [HttpGet("get-session/{id}")]
        [SwaggerOperation(Description = "Retrieve session by the Id")]
        public async Task<IActionResult> GetSession(Guid id)
        {
            try
            {
                var session = await _iSessionsRepository.GetSessionAsync(id);

                if (session == null)
                {
                    return NotFound(new
                    {
                        Message = SessionNotFound
                    });
                }

                return Ok(session).WithETag(session.ETag.ToString());
                
            }catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = InternalServerError
                });
            }
        }

        // PUT: api/Session/update-session/5
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("update-session/{id}")]
        [SwaggerOperation(Description = "Update session")]
        public async Task<IActionResult> PutSession(Guid id, UpdateSessionDto updateSessionDto)
        {
            if (id != updateSessionDto.Id)
            {
                return BadRequest(new
                {
                    Message = RouteIdAndSessionIdDontMatch
                });
            }

            // Ensures that the right validation option is used 
            var validation = _validator.Create(false);
            var result = await validation.ValidateAsync(updateSessionDto);

            if (!result.IsValid)
            {
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                result.AddToModelState(this.ModelState);

                // send model state to the client.
                return BadRequest(ModelState);
            }

            try
            {
                var existingSession = await _iSessionsRepository.GetSessionAsync(id);

                if (existingSession == null)
                {
                    return NotFound(new
                    {
                        Message = SessionAboutToBeUpdatedNotFound
                    });
                }

                // Ensure that the session being updated has the correct ETag
                var incomingEtag = Request.Headers["ETag"].ToString();

                if (existingSession.ETag.ToString() != incomingEtag)
                {
                    return StatusCode(412, new
                    {
                        Message = NotCorrectETag
                    });
                }

                // Kindly note that we have decided to do our updates here so that the UpdateSession method
                // in the InMemorySessionsRepository is only responsible for updating record (single responsibility). 
                // A business logic class will be perfect for this scenario
                existingSession.Duration = TimeSpan.Parse(updateSessionDto.Duration);
                existingSession.Tags = updateSessionDto.Tags;
                existingSession.ETag = Guid.NewGuid();
                existingSession.Name = updateSessionDto.Name;

                await _iSessionsRepository.UpdateSession(existingSession);

                // return NoContent().WithETag(existingSession.ETag.ToString());

                return Ok(existingSession).WithETag(existingSession.ETag.ToString());

            }catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException { Number: 2601 })
                {
                    _logger.LogError(NameExists);
                    return BadRequest(new
                    {
                        Message = NameExists
                    });

                }

                throw;
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    //Message = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message
                    Message = InternalServerError
                });
            }

        }

        // POST: api/Session/insert-session
        // To protect from over-posting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("insert-session")]
        [SwaggerOperation(Description = "Insert session")]
        public async Task<IActionResult> AddSession(AddSessionDto addSessionDto)
        {

            // Ensures that the right validation option is used 
            // isAdd is true if we are creating record
            var validation = _validator.Create(true);
            var result = await validation.ValidateAsync(addSessionDto);


            if (!result.IsValid)
            {
                // Copy the validation results into ModelState.
                // ASP.NET uses the ModelState collection to populate 
                // error messages in the View.
                result.AddToModelState(this.ModelState);

                // send model state to the client.
                return BadRequest(ModelState);

            }

            try
            {
                // Note that the value for the ETag is created implicitly 
                // whenever the Session constructor is called
                var session = _mapper.Map<Session>(addSessionDto);
                
                // Saving session into the database
                await _iSessionsRepository.AddSessionAsync(session);
                
                _logger.LogInformation(SessionCreated);
                
                // ETag is added using the extension method approach
                return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session)
                    .WithETag(session.ETag.ToString());

            }

            #region The DbException is required if we had followed "It's easier to ask forgiveness than permission"

            /*catch (DbException ex)
            {
                return BadRequest(new
                {
                    Message = ex?.InnerException?.InnerException?.Message ?? ex?.InnerException?.Message ?? ex?.Message
                });
            }*/

            #endregion

            catch (Exception)
            {
                return StatusCode(500, new
                {
                    /* This hides the exception error message from the user and just displays server error.
                     However for development purposes, this might useful.*/
                    // Message = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message
                    
                    Message = InternalServerError
                });
            }
        }

        // DELETE: api/Session/delete-session/5
        [HttpDelete("delete-session/{id}")]
        [SwaggerOperation(Description = "Delete session")]
        public async Task<IActionResult> DeleteSession(Guid id)
        {
            try
            {
                var session = await _iSessionsRepository.GetSessionAsync(id);
                if (session == null)
                {
                    return NotFound(new
                    {
                        Message = DeleteSessionNotFound
                    });
                }
                
                var incomingEtag = Request.Headers["ETag"].ToString();

                if (session.ETag.ToString() != incomingEtag)
                {
                    return StatusCode(412, new
                    {
                        Message = NotCorrectETag
                    });
                }
                
                await _iSessionsRepository.RemoveSessionAsync(session);

                return Ok(new
                {
                    Message = SessionDeleted
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = InternalServerError
                });
            }

        }
        
        [HttpGet("filter-sessions")]
        public async Task<IActionResult> ListSessions(string name = null, string tag = null, int page = 1, int pageSize = 10)
        {
            try
            {
                // Filter the sessions based on the provided parameters
                var sessions = await _iSessionsRepository.GetSessionsWithPaginationAsync(name, tag, page, pageSize);

                // Return the filtered sessions
                return Ok(new
                {
                    Sessions = sessions,
                    Pagination = new
                    {
                        Total = sessions.Count(),
                        Page = page,
                        PageSize = pageSize
                    }
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = InternalServerError
                });
            }
        }
        
        [HttpGet("average-duration")]
        public async Task<IActionResult> GetAverageSessionDuration(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Calculate the average duration of the filtered sessions
                var averageDuration = await _iSessionsRepository.GetAverageSessionDurationAsync(startDate, endDate);

                return Ok(new
                {
                    AverageTimeDuration = averageDuration
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = InternalServerError
                });
            }
        }

        // GET: api/Session/get-session-by-etag/5
        [HttpGet("get-session-by-etag/{id}")]
        [SwaggerOperation(Description = "Retrieve session by the ETag")]
        public async Task<IActionResult> GetSessionByEtag([SwaggerParameter(Description = "The Etag of the session to retrieve")] Guid id)
        {
            try
            {
                var session = await _iSessionsRepository.GetSessionByETagAsync(id);

                if (session == null)
                {
                    return NotFound(new
                    {
                        Message = SessionNotFound
                    });
                }

                return Ok(session).WithETag(session.ETag.ToString());

            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    Message = InternalServerError
                });
            }
        }

    }
}
