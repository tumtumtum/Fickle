using System;
using Microsoft.Owin.Hosting;

namespace Fickle.WebApi.TestSelfHost
{
	class Program
	{
		static void Main(string[] args)
		{
			var url = "http://localhost:4321";

			using (WebApp.Start<Startup>(url))
			{
				Console.WriteLine("Listening on {0}", url);
				Console.ReadLine();
			}
		}
	}
}
