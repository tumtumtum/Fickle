namespace Fickle.Expressions.Fluent
{
	public interface IExpressionScope<out T>
	{
		T End();
	}
}
