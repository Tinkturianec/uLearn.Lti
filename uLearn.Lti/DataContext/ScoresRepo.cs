using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using uLearn.Lti.Models;

namespace uLearn.Lti.DataContext
{
	internal class ScoresRepo
	{
		private readonly LtiDb db;

		public ScoresRepo()
		{
			db = new LtiDb();
		}

		public async Task Update(string userId, int value)
		{
			var score = Find(userId);

			if (score == null)
			{
				score = new Score
				{
					UserId = userId,
					Value = value
				};
			}
			else
				score.Value = value;

			db.Scores.AddOrUpdate(score);
			await db.SaveChangesAsync();
		}

		public Score Find(string userId)
		{
			return db.Scores.FirstOrDefault(score => score.UserId == userId);
		}
	}
}