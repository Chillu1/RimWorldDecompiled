using System;
using System.Xml;
using RimWorld;
using RimWorld.Planet;

namespace Verse;

public class BackCompatibilityConverter_1_2 : BackCompatibilityConverter
{
	public override bool AppliesToVersion(int majorVer, int minorVer)
	{
		return majorVer switch
		{
			1 => minorVer <= 2, 
			0 => true, 
			_ => false, 
		};
	}

	public override string BackCompatibleDefName(Type defType, string defName, bool forDefInjections = false, XmlNode node = null)
	{
		if (defType == typeof(RoyalTitleDef) && defName == "Esquire")
		{
			return "Acolyte";
		}
		if (defType == typeof(PawnKindDef) && defName == "Empire_Royal_Esquire")
		{
			return "Empire_Royal_Acolyte";
		}
		if (defType == typeof(HairDef) && defName == "ShavedFemale")
		{
			return "Shaved";
		}
		return null;
	}

	public override Type GetBackCompatibleType(Type baseType, string providedClassName, XmlNode node)
	{
		if (providedClassName == "Hediff_ImplantWithLevel")
		{
			return typeof(Hediff_Level);
		}
		return null;
	}

	public override void PostExposeData(object obj)
	{
		if (Scribe.mode == LoadSaveMode.LoadingVars)
		{
			if (obj is Building_Bed { CompAssignableToPawn: not null } building_Bed)
			{
				bool value = false;
				Scribe_Values.Look(ref value, "forPrisoners", defaultValue: false);
				if (value)
				{
					building_Bed.ForOwnerType = BedOwnerType.Prisoner;
				}
			}
			else if (obj is Pawn_GuestTracker pawn_GuestTracker)
			{
				bool value2 = false;
				Scribe_Values.Look(ref value2, "prisoner", defaultValue: false);
				if (value2)
				{
					pawn_GuestTracker.guestStatusInt = GuestStatus.Prisoner;
				}
			}
			else if (obj is QuestPart_RequirementsToAcceptNoDanger questPart_RequirementsToAcceptNoDanger)
			{
				Map refee = null;
				Scribe_References.Look(ref refee, "map");
				questPart_RequirementsToAcceptNoDanger.mapParent = refee.Parent;
			}
		}
		else
		{
			if (Scribe.mode != LoadSaveMode.PostLoadInit)
			{
				return;
			}
			if (obj is World world)
			{
				if (world.ideoManager == null)
				{
					world.ideoManager = new IdeoManager();
				}
				return;
			}
			Pawn pawn = obj as Pawn;
			if (pawn != null)
			{
				if (Find.World.ideoManager == null)
				{
					Find.World.ideoManager = new IdeoManager();
				}
				if (pawn.ownership == null)
				{
					pawn.ownership = new Pawn_Ownership(pawn);
				}
				if (!pawn.RaceProps.Humanlike)
				{
					return;
				}
				if (pawn.ideo == null)
				{
					pawn.ideo = new Pawn_IdeoTracker(pawn);
					if (pawn.Faction != null && pawn.Faction.def.humanlikeFaction && pawn.Faction.ideos == null)
					{
						pawn.Faction.ideos = new FactionIdeosTracker(pawn.Faction);
						pawn.Faction.ideos.ChooseOrGenerateIdeo(FactionIdeosTracker.IdeoGenerationParmsForFaction_BackCompatibility(pawn.Faction.def, !ModsConfig.IdeologyActive));
					}
					Ideo result;
					if (pawn.Faction?.ideos?.PrimaryIdeo != null)
					{
						pawn.ideo.SetIdeo(pawn.Faction.ideos.PrimaryIdeo);
					}
					else if (Find.IdeoManager.IdeosListForReading.TryRandomElementByWeight((Ideo x) => IdeoUtility.IdeoChangeToWeight(pawn, x), out result))
					{
						pawn.ideo.SetIdeo(result);
					}
				}
				if (pawn.style == null)
				{
					pawn.style = new Pawn_StyleTracker(pawn);
					pawn.style.beardDef = BeardDefOf.NoBeard;
				}
			}
			else if (obj is Faction faction)
			{
				if (Find.World.ideoManager == null)
				{
					Find.World.ideoManager = new IdeoManager();
				}
				if (faction.def.humanlikeFaction && faction.ideos == null)
				{
					faction.ideos = new FactionIdeosTracker(faction);
					faction.ideos.ChooseOrGenerateIdeo(FactionIdeosTracker.IdeoGenerationParmsForFaction_BackCompatibility(faction.def, !ModsConfig.IdeologyActive));
				}
			}
		}
	}
}
