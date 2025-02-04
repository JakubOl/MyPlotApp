﻿using AspNetCore.Identity.MongoDbCore.Models;

namespace Models.Entities
{
	public class UserModel : MongoIdentityUser<Guid>
	{
		public string UserId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string DisplayName { get; set; }
		public string EmailAddress { get; set; }
		public string MobileNumber { get; set; }
		public string Address { get; set; }
		public string Country { get; set; }
		public string StateRegion { get; set; }
		public string passwordHash { get; set; }
		public string Role { get; set; }
		public DateTime CreatedDate { get; set; } = DateTime.Now;
		public List<string> PlotsIds { get; set; } = new List<string>();
		public List<string> Auctions { get; set; } = new List<string>();
		public List<string> Watched { get; set; } = new List<string>();
		public List<string> Comments { get; set; } = new List<string>();
	}
}
