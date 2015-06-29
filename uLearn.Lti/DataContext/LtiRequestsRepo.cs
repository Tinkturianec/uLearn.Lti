using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LtiLibrary.Core.Lti1;
using Newtonsoft.Json;

namespace uLearn.Lti.DataContext
{
	internal class LtiRequestsRepo
	{
		private readonly LtiDb db;
		private readonly JsonSerializer serializer;

		public LtiRequestsRepo()
		{
			db = new LtiDb();
			serializer = new JsonSerializer();
		}

		public async Task Update(string userId, string ltiRequestJson)
		{
			var ltiRequestModel = FindElement(userId);

			if (ltiRequestModel == null)
			{
				ltiRequestModel = new LtiRequestModel
				{
					UserId = userId,
					Request = ltiRequestJson
				};
			}
			else
				ltiRequestModel.Request = ltiRequestJson;

			db.LtiRequests.AddOrUpdate(ltiRequestModel);
			await db.SaveChangesAsync();
		}

		public LtiRequest Find(string userId)
		{
			var ltiRequestModel = FindElement(userId);
			if (ltiRequestModel == null)
				return null;

			return serializer.Deserialize<LtiRequest>(new JsonTextReader(new StringReader(ltiRequestModel.Request)));
		}

		private LtiRequestModel FindElement(string userId)
		{
			return db.LtiRequests.FirstOrDefault(request => request.UserId == userId);
		}
	}
}