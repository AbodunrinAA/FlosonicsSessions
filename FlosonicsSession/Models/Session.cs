using System.ComponentModel.DataAnnotations;

namespace FlosonicsSession.Models
{

    public class Session
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ETag { get; set; }

        public DateTimeOffset DateAdded { get; set; }

        public string Tags { get; set; }

        public TimeSpan Duration { get; set; }

        public string Name { get; set; }

        public Session()
        {
            ETag = Guid.NewGuid();
        }
    }
}