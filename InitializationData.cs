namespace Ilera.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Ilera.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Ilera.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Ilera.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //

            

            
            context.Users.AddOrUpdate(new ApplicationUser[] { ApplicationUser.UnAssigned});
            context.Locations.AddOrUpdate(new LocationModel[] { LocationModel.UnAssigned });
            context.Pharmarcys.AddOrUpdate(new PharmarcyModel[] { PharmarcyModel.UnAssigned });
            context.ScanLaboratorys.AddOrUpdate(new ScanLabModel[] { ScanLabModel.UnAssigned });
            context.Hospitals.AddOrUpdate(new HospitalModel[] { HospitalModel.UnAssigned });
            ScanInteractionRequest UnAssigned = ScanInteractionRequest.getDefault();
            UnAssigned.InteractionInstitutionID = HospitalModel.UnAssigned.id;
            UnAssigned.PatientID = ApplicationUser.UnAssigned.Id.ToString();
            UnAssigned.PrimaryPractictionerID = ApplicationUser.UnAssigned.Id.ToString();
            context.ScanInteractionRequests.AddOrUpdate(new ScanInteractionRequest[] { UnAssigned });
        }
    }
}
