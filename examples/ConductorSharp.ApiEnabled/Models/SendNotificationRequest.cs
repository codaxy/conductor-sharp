using System.ComponentModel.DataAnnotations;

namespace ConductorSharp.ApiEnabled.Models
{
    public class SendNotificationRequest
    {
        [Required]
        public int CustomerId { get; set; }
    }
}
