using System;

namespace Fickle.Generators.CSharp
{
	public class CSharpAwaitedTaskType
		: FickleBaseType
	{
		public Type TaskElementType { get; private set; }

		public CSharpAwaitedTaskType(Type taskElementType)
			: base("CSharpAwaitedTaskType")
		{
			this.TaskElementType = taskElementType;
		}
	}
}
