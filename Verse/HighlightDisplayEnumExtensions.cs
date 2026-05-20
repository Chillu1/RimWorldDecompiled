using System;

namespace Verse;

public static class HighlightDisplayEnumExtensions
{
	public static string ToStringHuman(this DotHighlightDisplayMode mode)
	{
		return mode switch
		{
			DotHighlightDisplayMode.None => "None".Translate(), 
			DotHighlightDisplayMode.HighlightHostiles => "DotHighlightDisplayMode_HighlightHostiles".Translate(), 
			DotHighlightDisplayMode.HighlightAll => "DotHighlightDisplayMode_HighlightAll".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}

	public static string ToStringHuman(this HighlightStyleMode mode)
	{
		return mode switch
		{
			HighlightStyleMode.Dots => "HighlightStyleMode_Dots".Translate(), 
			HighlightStyleMode.Silhouettes => "HighlightStyleMode_Silhouettes".Translate(), 
			_ => throw new NotImplementedException(), 
		};
	}
}
