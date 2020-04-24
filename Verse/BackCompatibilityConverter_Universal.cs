using RimWorld;
using System;
using System.Xml;

namespace Verse
{
	public class BackCompatibilityConverter_Universal : BackCompatibilityConverter
	{
		public override bool AppliesToVersion(int majorVer, int minorVer)
		{
			return true;
		}

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef) && (defName == "WoolYak" || defName == "WoolCamel"))
			{
				return "WoolSheep";
			}
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			return null;
		}

		public override void PostExposeData(object obj)
		{
			Pawn_RoyaltyTracker pawn_RoyaltyTracker;
			if (Scribe.mode == LoadSaveMode.PostLoadInit && (pawn_RoyaltyTracker = (obj as Pawn_RoyaltyTracker)) != null && VersionControl.BuildFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion) <= 2575)
			{
				foreach (RoyalTitle item in pawn_RoyaltyTracker.AllTitlesForReading)
				{
					item.conceited = RoyalTitleUtility.ShouldBecomeConceitedOnNewTitle(pawn_RoyaltyTracker.pawn);
				}
			}
		}
	}
}
