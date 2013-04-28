namespace Coderoom.LoadBalancer.Utilities
{
	public static class StringExtensions
	{
		public static string Fmt(this string value, params object[] args)
		{
			return string.Format(value, args);
		}
	}
}