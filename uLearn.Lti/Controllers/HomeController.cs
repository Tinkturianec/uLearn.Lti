using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using LtiLibrary.Core.Lti1;
using LtiLibrary.Core.Outcomes.v1;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using uLearn.Lti.DataContext;

namespace uLearn.Lti.Controllers
{
	public class HomeController : Controller
	{
		private readonly LtiRequestsRepo ltiRequestsRepo = new LtiRequestsRepo();
		private readonly ScoresRepo scoreRepo = new ScoresRepo();
		private readonly ConsumersRepo consumersRepo = new ConsumersRepo();

		[Authorize]
		public async Task<ViewResult> Index()
		{
			var userId = User.Identity.GetUserId();
			var ltiRequest = FindLtiRequest();
			var ltiRequestJson = FindLtiRequestJson();

			if (!string.IsNullOrWhiteSpace(ltiRequestJson))
				await ltiRequestsRepo.Update(userId, ltiRequestJson);

			return View(ltiRequest != null ? ltiRequest.Parameters : new NameValueCollection());
		}

		[Authorize]
		public async Task Score(int score)
		{
			if (score < 0 || 100 < score)
				throw new Exception("score must be in range [0 .. 100], but was " + score);

			var userId = User.Identity.GetUserId();
			await scoreRepo.Update(userId, score);
		}

		[Authorize]
		public void SubmitScore()
		{
			var userId = User.Identity.GetUserId();

			var score = scoreRepo.Find(userId);
			if (score == null)
				throw new Exception("Score for user '" + userId + "' not found");

			var ltiRequest = ltiRequestsRepo.Find(userId);
			if (ltiRequest == null)
				throw new Exception("LtiRequest for user '" + userId + "' not found");

			var consumerSecret = consumersRepo.Find(ltiRequest.ConsumerKey).Secret;


			// TODO: fix outcome address in local edx (no localhost and no https)
			var uri = new UriBuilder(ltiRequest.LisOutcomeServiceUrl);
			if (uri.Host == "localhost")
			{
				uri.Host = "192.168.33.10";
				uri.Port = 80;
				uri.Scheme = "http";
			}

			var result = OutcomesClient.PostScore(uri.ToString(), ltiRequest.ConsumerKey, consumerSecret,
				ltiRequest.LisResultSourcedId, score.Value/100.0);

			if (!result.IsValid)
				throw new Exception(uri.ToString() + "\r\n\r\n" + result.Message);
		}

		private string FindLtiRequestJson()
		{
			var user = User.Identity as ClaimsIdentity;
			if (user == null)
				return null;

			var claim = user.Claims.FirstOrDefault(c => c.Type.Equals("LtiRequest"));
			return claim == null ? null : claim.Value;
		}

		private LtiRequest FindLtiRequest()
		{
			var ltiRequestJson = FindLtiRequestJson();
			if (ltiRequestJson == null)
				return null;

			return JsonSerializer.Create().Deserialize<LtiRequest>(new JsonTextReader(new StringReader(ltiRequestJson)));
		}
	}
}