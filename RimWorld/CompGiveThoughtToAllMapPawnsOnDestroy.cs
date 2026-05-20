using Verse;

namespace RimWorld;

public class CompGiveThoughtToAllMapPawnsOnDestroy : ThingComp
{
	private CompProperties_GiveThoughtToAllMapPawnsOnDestroy Props => (CompProperties_GiveThoughtToAllMapPawnsOnDestroy)props;

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		if ((mode == DestroyMode.Vanish && Props.ignoreOnVanish) || (Props.onlyWhenKilled && mode != DestroyMode.KillFinalize && mode != DestroyMode.KillFinalizeLeavingsOnly) || previousMap == null)
		{
			return;
		}
		if (!Props.message.NullOrEmpty())
		{
			Messages.Message(Props.message, new TargetInfo(parent.Position, previousMap), MessageTypeDefOf.NegativeEvent);
		}
		foreach (Pawn item in previousMap.mapPawns.AllPawnsSpawned)
		{
			item.needs?.mood?.thoughts.memories.TryGainMemory(Props.thought);
		}
	}
}
