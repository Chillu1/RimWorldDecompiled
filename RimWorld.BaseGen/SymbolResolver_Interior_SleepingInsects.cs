using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Interior_SleepingInsects : SymbolResolver
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
			Mathf.Min(rp.rect.Width, rp.rect.Height);
			LordJob_SleepThenAssaultColony lordJob_SleepThenAssaultColony = new LordJob_SleepThenAssaultColony(Faction.OfInsects);
			lordJob_SleepThenAssaultColony.awakeOnClamor = true;
			Lord lord = LordMaker.MakeNewLord(Faction.OfInsects, lordJob_SleepThenAssaultColony, BaseGen.globalSettings.map);
			foreach (PawnKindDef combatPawnKindsForPoint in PawnUtility.GetCombatPawnKindsForPoints((PawnKindDef k) => k.RaceProps.Insect, rp.threatPoints.Value))
			{
				ResolveParams resolveParams = rp;
				resolveParams.faction = Faction.OfInsects;
				resolveParams.singlePawnKindDef = combatPawnKindsForPoint;
				resolveParams.singlePawnLord = lord;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
			SignalAction_DormancyWakeUp obj = (SignalAction_DormancyWakeUp)ThingMaker.MakeThing(ThingDefOf.SignalAction_DormancyWakeUp);
			obj.signalTag = rp.sleepingInsectsWakeupSignalTag;
			obj.lord = lord;
			GenSpawn.Spawn(obj, rp.rect.CenterCell, BaseGen.globalSettings.map);
		}
	}
}
