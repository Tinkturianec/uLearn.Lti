using System.Linq;
using uLearn.Lti.Models;

namespace uLearn.Lti.DataContext
{
	internal class ConsumersRepo
	{
		private readonly LtiDb db;

		public ConsumersRepo()
			: this(new LtiDb())
		{

		}

		public ConsumersRepo(LtiDb db)
		{
			this.db = db;
		}

		public Consumer Find(string consumerKey)
		{
			return db.Consumers.SingleOrDefault(consumer => consumer.Key == consumerKey);
		}

	}
}