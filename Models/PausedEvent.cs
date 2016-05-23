using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    [Table("PausedEvent")]
    public class PausedEvent
    {
        [Column(Order = 0), ForeignKey("User")]
        public string UserId { get; set; }
        ApplicationUser _User;
        /// <summary>
        /// User for which the event is associated
        /// </summary>
        public ApplicationUser User {
            get {
                return _User;
            }
            set {
                _User =value;
            }
        }

        /// <summary>
        /// Id of the sub evnet that is being paused
        /// </summary>
        [Column(Order = 1), Key]
        public string EventId { get; set; }
        /// <summary>
        /// When the event is paused. Accuracy is to the minute
        /// </summary>
        public DateTimeOffset PauseTime { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// Is the current event paused
        /// </summary>
        public bool isPauseDeleted { get; set; } = false;
    }
}