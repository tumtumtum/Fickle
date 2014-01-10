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
		private ServiceClass CreatePersonType()
		{
			return new ServiceClass()
			{
				Name = "Person",
				Properties = new List<ServiceProperty>()
				{
					new ServiceProperty()
					{
						Name = "Id",
						TypeName = "Guid"
					},
					new ServiceProperty()
					{
						Name = "Name",
						TypeName = "String"
					},
					new ServiceProperty()
					{
						Name = "BirthDate",
						TypeName = "DateTime"
					},
					new ServiceProperty()
					{
						Name = "TimeAwake",
						TypeName = "TimeSpan"
					},
					new ServiceProperty()
					{
						Name = "NullableLengthInMicrons",
						TypeName = "Long?"
					},
					new ServiceProperty()
					{
						Name = "LengthInMicrons",
						TypeName = "Long"
					},
					new ServiceProperty()
					{
						Name = "NullableLengthInMicrons",
						TypeName = "Long?"
					},
					new ServiceProperty()
					{
						Name = "Age",
						TypeName = "Int"
					},
					new ServiceProperty()
					{
						Name = "NullableAge",
						TypeName = "Int?"
					},
					new ServiceProperty()
					{
						Name = "NewPassword",
						TypeName = "String"
					},
					new ServiceProperty()
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
			
			serviceModel.Classes = new List<ServiceClass>
			{
				new ServiceClass { Name = "ServiceObject" },
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
