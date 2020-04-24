using System;

namespace Verse
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class LoadAliasAttribute : Attribute
	{
		public string alias;

		public LoadAliasAttribute(string alias)
		{
			this.alias = alias;
		}
	}
}
