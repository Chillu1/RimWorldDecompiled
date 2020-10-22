using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FactionRelationKindUtility
	{
		public static string GetLabel(this FactionRelationKind kind)
		{
			return kind switch
			{
				FactionRelationKind.Hostile => "Hostile".Translate(), 
				FactionRelationKind.Neutral => "Neutral".Translate(), 
				FactionRelationKind.Ally => "Ally".Translate(), 
				_ => "error", 
			};
		}

		public static Color GetColor(this FactionRelationKind kind)
		{
			return kind switch
			{
				FactionRelationKind.Hostile => ColoredText.RedReadable, 
				FactionRelationKind.Neutral => new Color(0f, 0.75f, 1f), 
				FactionRelationKind.Ally => Color.green, 
				_ => Color.white, 
			};
		}
	}
}
