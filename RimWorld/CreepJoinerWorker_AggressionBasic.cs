using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class CreepJoinerWorker_AggressionBasic : BaseCreepJoinerWorker
{
	public override void DoResponse(List<TargetInfo> looktargets, List<NamedArgument> namedArgs)
	{
		base.Pawn.SetFaction(Find.FactionManager.OfEntities);
		base.Pawn.guest.Recruitable = false;
		base.Pawn.GetLord()?.RemovePawn(base.Pawn);
		Lord lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_AssaultColony(Faction.OfEntities, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false), base.Pawn.MapHeld);
		lord.AddPawn(base.Pawn);
	}
}
