using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_RequirementsToAcceptNoDanger : QuestPart_RequirementsToAccept
{
	public MapParent mapParent;

	public Pawn mapPawn;

	public Faction dangerTo;

	public Map TargetMap
	{
		get
		{
			if (mapParent != null && mapParent.HasMap)
			{
				return mapParent.Map;
			}
			if (mapPawn != null)
			{
				return mapPawn.MapHeld;
			}
			return null;
		}
	}

	public override IEnumerable<GlobalTargetInfo> Culprits
	{
		get
		{
			if (TargetMap != null && GenHostility.AnyHostileActiveThreatTo(TargetMap, dangerTo, out var threat))
			{
				yield return (Thing)threat;
			}
		}
	}

	public override AcceptanceReport CanAccept()
	{
		if (TargetMap != null && GenHostility.AnyHostileActiveThreatTo(TargetMap, dangerTo))
		{
			return new AcceptanceReport("QuestRequiresNoDangerOnMap".Translate());
		}
		return true;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_References.Look(ref mapPawn, "mapPawn");
		Scribe_References.Look(ref dangerTo, "dangerTo");
	}
}
