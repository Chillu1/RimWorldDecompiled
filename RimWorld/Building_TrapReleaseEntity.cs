using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Sound;

namespace RimWorld;

public abstract class Building_TrapReleaseEntity : Building_Trap
{
	protected abstract PawnKindDef PawnToSpawn { get; }

	protected abstract int CountToSpawn { get; }

	public override bool ShouldShowTrapDamageStat => false;

	protected override void Tick()
	{
		base.Tick();
		if (!base.Spawned || base.IsStunned || !this.IsHashIntervalTick(60))
		{
			return;
		}
		Map map = base.Map;
		IEnumerable<Pawn> enumerable = map.mapPawns.AllPawnsSpawned.Where((Pawn x) => !x.IsPsychologicallyInvisible() && x.HostileTo(this) && x.Position.DistanceTo(base.Position) <= 20f);
		IntVec3 position = base.Position;
		foreach (Pawn item in enumerable)
		{
			if ((base.Faction != Faction.OfPlayer || (!item.IsPrisoner && !item.Downed)) && GenSight.LineOfSight(position, item.Position, map))
			{
				Spring(item);
				break;
			}
		}
	}

	public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
	{
		if (base.IsStunned)
		{
			base.Kill(dinfo, exactCulprit);
		}
		else
		{
			Spring(dinfo?.Instigator as Pawn);
		}
	}

	protected override void SpringSub(Pawn p)
	{
		if (base.Spawned)
		{
			SoundDefOf.DroneTrapSpring.PlayOneShot(new TargetInfo(base.Position, base.Map));
			if (base.Faction != Faction.OfPlayer)
			{
				Messages.Message("MessageTrapSprung".Translate(this.Named("TRAP")), new LookTargets(base.Position, base.Map), MessageTypeDefOf.NegativeEvent);
			}
			for (int i = 0; i < CountToSpawn; i++)
			{
				PawnKindDef pawnToSpawn = PawnToSpawn;
				Faction faction = base.Faction;
				float? fixedBiologicalAge = 0f;
				float? fixedChronologicalAge = 0f;
				Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnToSpawn, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, fixedBiologicalAge, fixedChronologicalAge));
				pawn.mindState.enemyTarget = p;
				GenSpawn.Spawn(pawn, base.Position, base.Map);
			}
		}
	}

	protected override float SpringChance(Pawn p)
	{
		return 0f;
	}
}
