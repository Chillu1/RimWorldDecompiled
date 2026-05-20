using System;
using System.Collections.Generic;
using System.Xml;
using RimWorld;
using RimWorld.Planet;

namespace Verse
{
	public class BackCompatibilityConverter_1_5 : BackCompatibilityConverter
	{
		public override bool AppliesToVersion(int majorVer, int minorVer)
		{
			return majorVer switch
			{
				1 => minorVer <= 5, 
				0 => true, 
				_ => false, 
			};
		}

		public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
		{
			if (defType == typeof(ThingDef) && defName == "DropPodIncomingMechanoid")
			{
				return "DropPodIncomingMechanoidRapid";
			}
			if (defType == typeof(ThingDef) && defName == "AncientContainer")
			{
				return "AncientLargeContainer";
			}
			if (defType == typeof(IssueDef) && defName == "SmallSpaces")
			{
				return "Indoors";
			}
			if (defType == typeof(PreceptDef) && defName == "SmallSpaces_Acceptable")
			{
				return "Indoors_Acceptable";
			}
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			if (baseType == typeof(Area) && providedClassName == "Area_SnowClear")
			{
				return typeof(Area_SnowOrSandClear);
			}
			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (obj is WorldGenData worldGenData)
				{
					List<int> list = new List<int>();
					Scribe_Collections.Look(ref list, "roadNodes", LookMode.Undefined);
					List<PlanetTile> list2 = (worldGenData.roadNodes[Find.WorldGrid.Surface] = new List<PlanetTile>());
					List<PlanetTile> list4 = list2;
					foreach (int item in list)
					{
						list4.Add(new PlanetTile(item, Find.WorldGrid.Surface));
					}
				}
				if (obj is Quest quest)
				{
					int value = -1;
					Scribe_Values.Look(ref value, "ticksUntilAcceptanceExpiry", -1);
					if (value >= 0)
					{
						quest.acceptanceExpireTick = GenTicks.TicksGame + value;
					}
				}
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && obj is WorldInfo)
			{
				WorldGenStep_Mutators.AddMutatorsFromTile(Find.WorldGrid.Surface);
			}
		}
	}
}
