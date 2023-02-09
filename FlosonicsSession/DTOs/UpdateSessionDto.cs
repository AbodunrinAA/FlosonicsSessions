using System.ComponentModel;

namespace FlosonicsSession.DTOs;

public class UpdateSessionDto: ISessionDto
{
    public Guid Id { get; set; }
    
    [DefaultValue("aa,bb,cc")]
    public string Tags { get; set; }
    
    [DefaultValue("00:01:00")]
    public string Duration { get; set; }
    
    [DefaultValue("name")]
    public string Name { get; set; }
}