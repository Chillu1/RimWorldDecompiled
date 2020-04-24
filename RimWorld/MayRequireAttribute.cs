using System;

namespace RimWorld
{
	[AttributeUsage(AttributeTargets.Field)]
	public class MayRequireAttribute : Attribute
	{
		public string modId;

		public MayRequireAttribute(string modId)
		{
			this.modId = modId;
		}
	}
}
