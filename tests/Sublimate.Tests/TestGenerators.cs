using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sublimate.Generators.Objective;
using Sublimate.Model;

namespace Sublimate.Tests
{
	[TestFixture]
	public class TestGenerators
	{
		private ServiceType CreatePersonType()
		{
			return new ServiceType()
			{
				Name = "Person",
				Properties = new List<ServiceTypeProperty>()
				{
					new ServiceTypeProperty()
					{
						Name = "Id",
						TypeName = "Guid"
					},
					new ServiceTypeProperty()
					{
						Name = "Name",
						TypeName = "String"
					},
					new ServiceTypeProperty()
					{
						Name = "BirthDate",
						TypeName = "DateTime"
					},
					new ServiceTypeProperty()
					{
						Name = "TimeAwake",
						TypeName = "TimeSpan"
					},
					new ServiceTypeProperty()
					{
						Name = "NullableLengthInMicrons",
						TypeName = "Long?"
					},
					new ServiceTypeProperty()
					{
						Name = "LengthInMicrons",
						TypeName = "Long"
					},
					new ServiceTypeProperty()
					{
						Name = "NullableLengthInMicrons",
						TypeName = "Long?"
					},
					new ServiceTypeProperty()
					{
						Name = "Age",
						TypeName = "Int"
					},
					new ServiceTypeProperty()
					{
						Name = "NullableAge",
						TypeName = "Int?"
					},
					new ServiceTypeProperty()
					{
						Name = "NewPassword",
						TypeName = "String"
					},
					new ServiceTypeProperty()
					{
						Name = "Friend",
						TypeName = "Person"
					}
				}
			};
		}

		[Test]
		public void Test_GenerateType()
		{
			var serviceModel = new ServiceModel();
			var personType = this.CreatePersonType();
			
			serviceModel.Types = new List<ServiceType>
			{
				new ServiceType { Name = "ServiceObject" },
				this.CreatePersonType()
			};

			var headerExpression = new ServiceExpressionBuilder(serviceModel).Build(personType);
			headerExpression = ObjectiveHeaderExpressionBinder.Bind(headerExpression);

			var classExpression = new ServiceExpressionBuilder(serviceModel).Build(personType);
			classExpression = ObjectiveClassExpressionBinder.Bind(classExpression);

			var objectiveCodeGenerator = new ObjectiveCodeGenerator(Console.Out);

			objectiveCodeGenerator.Generate(headerExpression);
			Console.WriteLine();
			objectiveCodeGenerator.Generate(classExpression);
		}
	}
}
