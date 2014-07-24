using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace DryIce.WebApi.TestWebService.ServiceModel
{
	public class Person
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}
}