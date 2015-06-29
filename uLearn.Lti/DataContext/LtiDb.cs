using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using uLearn.Lti.Migrations;
using uLearn.Lti.Models;

namespace uLearn.Lti.DataContext
{
	public class LtiDb : IdentityDbContext<ApplicationUser>
	{
		public LtiDb()
			: base("DefaultConnection")
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<LtiDb, Configuration>());

		}

		public DbSet<Consumer> Consumers { get; set; }
		public DbSet<LtiRequestModel> LtiRequests { get; set; }
		public DbSet<Score> Scores { get; set; }
	}
}