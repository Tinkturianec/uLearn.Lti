using System.Data.Entity.Migrations;
using uLearn.Lti.DataContext;

namespace uLearn.Lti.Migrations
{
	internal sealed class Configuration : DbMigrationsConfiguration<LtiDb>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = false;
		}

		protected override void Seed(LtiDb context)
		{
		}
	}
}