using RimWorld;
using System;
using System.Xml;

namespace Verse
{
	public class BackCompatibilityConverter_0_19 : BackCompatibilityConverter
	{
		public override bool AppliesToVersion(int majorVer, int minorVer)
		{
			if (majorVer == 0)
			{
				return minorVer <= 19;
			}
			return false;
		}

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Game game = obj as Game;
				if (game != null && game.foodRestrictionDatabase == null)
				{
					game.foodRestrictionDatabase = new FoodRestrictionDatabase();
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				Pawn pawn = obj as Pawn;
				if (pawn != null && pawn.foodRestriction == null && pawn.RaceProps.Humanlike && ((pawn.Faction != null && pawn.Faction.IsPlayer) || (pawn.HostFaction != null && pawn.HostFaction.IsPlayer)))
				{
					pawn.foodRestriction = new Pawn_FoodRestrictionTracker(pawn);
				}
			}
		}
	}
}
