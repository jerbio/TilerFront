using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TilerFront.Models
{
    public class TilerContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public TilerContext(): base("name=TilerContext")
        {
        }

        public System.Data.Entity.DbSet<TilerFront.Models.ApplicationUser> Users { get; set; }

        public System.Data.Entity.DbSet<TilerFront.Models.SubCalEvent> SubCalEvents { get; set; }

        public System.Data.Entity.DbSet<TilerFront.Models.CalEvent> CalEvents { get; set; }
    
    }
}
