using System;
using Verse;

namespace RimWorld;

public static class MonolithGenerationMethodExtension
{
	public static string ToStringHuman(this MonolithGenerationMethod method)
	{
		return method switch
		{
			MonolithGenerationMethod.Disabled => "MonolithGenerationMethod_Disabled".Translate(), 
			MonolithGenerationMethod.NearColonists => "MonolithGenerationMethod_NearColonists".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}
}
