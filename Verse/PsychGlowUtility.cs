using System;

namespace Verse;

public static class PsychGlowUtility
{
	public static string GetLabel(this PsychGlow gl)
	{
		return gl switch
		{
			PsychGlow.Dark => "Dark".Translate(), 
			PsychGlow.Lit => "Lit".Translate(), 
			PsychGlow.Overlit => "LitBrightly".Translate(), 
			_ => throw new ArgumentException(), 
		};
	}
}
