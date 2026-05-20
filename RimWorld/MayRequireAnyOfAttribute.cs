using System;

namespace RimWorld
{
	[AttributeUsage(AttributeTargets.Field)]
	public class MayRequireAnyOfAttribute : Attribute
	{
		public string[] modIds;

		public MayRequireAnyOfAttribute(string modId)
		{
			modIds = modId.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < modIds.Length; i++)
			{
				modIds[i] = modIds[i].Trim();
			}
		}
	}
}
