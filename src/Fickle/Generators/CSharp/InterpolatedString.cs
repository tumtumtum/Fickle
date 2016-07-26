namespace Fickle.Generators.CSharp
{
	public class InterpolatedString
	{
		public string Value { get; private set; }

		public InterpolatedString(string value)
		{
			this.Value = value;
		}
	}
}
