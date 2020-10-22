using System;
using System.Collections.Generic;
using System.Xml;
using RimWorld;

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
			if (defType == typeof(ThingDef))
			{
				switch (defName)
				{
				case "WoolYak":
				case "WoolCamel":
					return "WoolSheep";
				case "Plant_TreeAnimus":
				case "Plant_TreeAnimusSmall":
				case "Plant_TreeAnimaSmall":
				case "Plant_TreeAnimaNormal":
				case "Plant_TreeAnimaHardy":
					return "Plant_TreeAnima";
				case "Psytrainer_EntropyLink":
					return "Psytrainer_EntropyDump";
				case "PsylinkNeuroformer":
					return "PsychicAmplifier";
				case "PsychicShockLance":
					return "Apparel_PsychicShockLance";
				case "PsychicInsanityLance":
					return "Apparel_PsychicInsanityLance";
				}
			}
			if (defType == typeof(AbilityDef) && defName == "EntropyLink")
			{
				return "EntropyDump";
			}
			if (defType == typeof(HediffDef) && defName == "Psylink")
			{
				return "PsychicAmplifier";
			}
			return null;
		}

		public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
		{
			switch (providedClassName)
			{
			case "Hediff_PsychicAmplifier":
				return typeof(Hediff_Psylink);
			case "ThingWithComps":
			case "Verse.ThingWithComps":
			{
				XmlElement xmlElement = node["def"];
				if (xmlElement != null)
				{
					if (xmlElement.InnerText == "PsychicShockLance")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "PsychicInsanityLance")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "OrbitalTargeterBombardment")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "OrbitalTargeterPowerBeam")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "OrbitalTargeterMechCluster")
					{
						return typeof(Apparel);
					}
					if (xmlElement.InnerText == "TornadoGenerator")
					{
						return typeof(Apparel);
					}
				}
				break;
			}
			}
			return null;
		}

		public override void PostExposeData(object obj)
		{
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				int num = VersionControl.BuildFromVersionString(ScribeMetaHeaderUtility.loadedGameVersion);
				Pawn_RoyaltyTracker pawn_RoyaltyTracker;
				if ((pawn_RoyaltyTracker = obj as Pawn_RoyaltyTracker) != null && num <= 2575)
				{
					foreach (RoyalTitle item in pawn_RoyaltyTracker.AllTitlesForReading)
					{
						item.conceited = RoyalTitleUtility.ShouldBecomeConceitedOnNewTitle(pawn_RoyaltyTracker.pawn);
					}
				}
				Pawn_NeedsTracker pawn_NeedsTracker;
				if ((pawn_NeedsTracker = obj as Pawn_NeedsTracker) != null)
				{
					pawn_NeedsTracker.AllNeeds.RemoveAll((Need n) => n.def.defName == "Authority");
				}
			}
			Pawn pawn;
			Map map;
			if ((pawn = obj as Pawn) != null)
			{
				if (pawn.abilities == null)
				{
					pawn.abilities = new Pawn_AbilityTracker(pawn);
				}
				if (pawn.health != null)
				{
					if (pawn.health.hediffSet.hediffs.RemoveAll((Hediff x) => x == null) != 0)
					{
						Log.Error(pawn.ToStringSafe() + " had some null hediffs.");
					}
					Hediff hediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.PsychicHangover);
					if (hediff != null)
					{
						pawn.health.hediffSet.hediffs.Remove(hediff);
					}
					Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.WakeUpTolerance);
					if (firstHediffOfDef != null)
					{
						pawn.health.hediffSet.hediffs.Remove(firstHediffOfDef);
					}
					Hediff firstHediffOfDef2 = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.GoJuiceTolerance);
					if (firstHediffOfDef2 != null)
					{
						pawn.health.hediffSet.hediffs.Remove(firstHediffOfDef2);
					}
				}
				if (pawn.equipment == null || pawn.apparel == null || pawn.inventory == null)
				{
					return;
				}
				List<ThingWithComps> list = null;
				for (int i = 0; i < pawn.equipment.AllEquipmentListForReading.Count; i++)
				{
					ThingWithComps thingWithComps = pawn.equipment.AllEquipmentListForReading[i];
					if (thingWithComps.def.defName == "OrbitalTargeterBombardment" || thingWithComps.def.defName == "OrbitalTargeterPowerBeam" || thingWithComps.def.defName == "OrbitalTargeterMechCluster" || thingWithComps.def.defName == "TornadoGenerator")
					{
						list = list ?? new List<ThingWithComps>();
						list.Add(thingWithComps);
					}
				}
				if (list == null)
				{
					return;
				}
				foreach (Apparel item2 in list)
				{
					pawn.equipment.Remove(item2);
					ResetVerbs(item2);
					if (pawn.apparel.CanWearWithoutDroppingAnything(item2.def))
					{
						pawn.apparel.Wear(item2);
					}
					else
					{
						pawn.inventory.innerContainer.TryAdd(item2);
					}
				}
			}
			else if (Scribe.mode == LoadSaveMode.LoadingVars && (map = obj as Map) != null && map.temporaryThingDrawer == null)
			{
				map.temporaryThingDrawer = new TemporaryThingDrawer();
			}
		}

		private void ResetVerbs(ThingWithComps t)
		{
			(t as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
			foreach (ThingComp allComp in t.AllComps)
			{
				(allComp as IVerbOwner)?.VerbTracker?.VerbsNeedReinitOnLoad();
			}
		}
	}
}
