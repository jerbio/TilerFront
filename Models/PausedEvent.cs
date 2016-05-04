using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    [Table("PausedEvent")]
    public class PausedEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
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
        public string EventId { get; set; }
        /// <summary>
        /// When the event is paused. Accuracy is to the minute
        /// </summary>
        public DateTimeOffset PauseTime { get; set; } = DateTimeOffset.UtcNow;
        public bool isPauseDeleted { get; set; } = false;
    }
}