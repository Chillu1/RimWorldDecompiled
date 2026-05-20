using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen;

public class SymbolResolver_Interior_SleepingMechanoids : SymbolResolver
{
	public override bool CanResolve(ResolveParams rp)
	{
		if (base.CanResolve(rp))
		{
			return rp.threatPoints.HasValue;
		}
		return false;
	}

	public override void Resolve(ResolveParams rp)
	{
		LordJob_SleepThenAssaultColony lordJob_SleepThenAssaultColony = new LordJob_SleepThenAssaultColony(Faction.OfMechanoids, rp.sendWokenUpMessage ?? true);
		lordJob_SleepThenAssaultColony.awakeOnClamor = true;
		Lord lord = LordMaker.MakeNewLord(Faction.OfMechanoids, lordJob_SleepThenAssaultColony, BaseGen.globalSettings.map);
		foreach (PawnKindDef combatPawnKindsForPoint in PawnUtility.GetCombatPawnKindsForPoints(MechClusterGenerator.MechKindSuitableForCluster, rp.threatPoints.Value, (PawnKindDef pk) => 1f / pk.combatPower))
		{
			ResolveParams resolveParams = rp;
			resolveParams.singlePawnKindDef = combatPawnKindsForPoint;
			resolveParams.singlePawnLord = lord;
			resolveParams.faction = Faction.OfMechanoids;
			BaseGen.symbolStack.Push("pawn", resolveParams);
		}
		if (rp.sleepingMechanoidsWakeupSignalTag != null)
		{
			SignalAction_DormancyWakeUp obj = (SignalAction_DormancyWakeUp)ThingMaker.MakeThing(ThingDefOf.SignalAction_DormancyWakeUp);
			obj.signalTag = rp.sleepingMechanoidsWakeupSignalTag;
			obj.lord = lord;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}
}
