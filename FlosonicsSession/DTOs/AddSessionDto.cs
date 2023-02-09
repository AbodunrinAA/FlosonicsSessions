using System.ComponentModel;

namespace FlosonicsSession.DTOs;

public class AddSessionDto: ISessionDto
{
    [DefaultValue("name")]
    public string Name { get; set; }
    
    [DefaultValue("aa,bb,cc")]
    public string Tags { get; set; }
    
    [DefaultValue("00:01:00")]
    public string Duration { get; set; }

}

