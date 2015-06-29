using System.ComponentModel.DataAnnotations;

namespace uLearn.Lti.Models
{
	public class Score
	{
		[Key]
		public string UserId { get; set; }

		[Required]
		public int Value { get; set; }
	}
}