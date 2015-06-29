using System.ComponentModel.DataAnnotations;

namespace uLearn.Lti.DataContext
{
	public class LtiRequestModel
	{
		[Key]
		public int RequestId { get; set; }

		[Required]
		[StringLength(64)]
		public string UserId { get; set; }

		[Required]
		public string Request { get; set; }
	}
}