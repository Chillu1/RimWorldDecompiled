using System;

namespace RimWorld
{
	[AttributeUsage(AttributeTargets.Field)]
	public class DefAliasAttribute : Attribute
	{
		public string defName;

		public DefAliasAttribute(string defName)
		{
			this.defName = defName;
		}
	}
}
