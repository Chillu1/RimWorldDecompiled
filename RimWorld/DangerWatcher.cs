using System;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class DangerWatcher
{
	private Map map;

	private StoryDanger dangerRatingInt;

	private int lastUpdateTick = -10000;

	private int lastColonistHarmedTick = -10000;

	private const int UpdateInterval = 101;

	private static readonly Func<IAttackTarget, bool> CachedAffectsStoryDangerDelegate = AffectsStoryDanger;

	public StoryDanger DangerRating
	{
		get
		{
			if (Find.TickManager.TicksGame > lastUpdateTick + 101)
			{
				dangerRatingInt = CalculateDangerRating();
				lastUpdateTick = Find.TickManager.TicksGame;
			}
			return dangerRatingInt;
		}
	}

	private StoryDanger CalculateDangerRating()
	{
		float num = map.attackTargetsCache.TargetsHostileToColony.Where(CachedAffectsStoryDangerDelegate).Sum(delegate(IAttackTarget t)
		{
			if (t is Pawn pawn)
			{
				return pawn.kindDef.combatPower;
			}
			return (t is Building_TurretGun building_TurretGun && building_TurretGun.def.building.IsMortar && !building_TurretGun.IsMannable) ? building_TurretGun.def.building.combatPower : 0f;
		});
		if (num == 0f)
		{
			return StoryDanger.None;
		}
		int num2 = map.mapPawns.FreeColonistsSpawned.Count((Pawn p) => !p.Downed);
		if (num < 150f && num <= (float)num2 * 18f)
		{
			return StoryDanger.Low;
		}
		if (num > 400f)
		{
			return StoryDanger.High;
		}
		if (lastColonistHarmedTick > Find.TickManager.TicksGame - 900)
		{
			return StoryDanger.High;
		}
		foreach (Lord lord in map.lordManager.lords)
		{
			if (lord.faction.HostileTo(Faction.OfPlayer) && lord.CurLordToil.ForceHighStoryDanger && lord.AnyActivePawn)
			{
				return StoryDanger.High;
			}
		}
		return StoryDanger.Low;
	}

	public DangerWatcher(Map map)
	{
		this.map = map;
	}

	public void Notify_ColonistHarmedExternally()
	{
		lastColonistHarmedTick = Find.TickManager.TicksGame;
	}

	private static bool AffectsStoryDanger(IAttackTarget t)
	{
		if (t.Thing is Pawn pawn)
		{
			Lord lord = pawn.GetLord();
			if (lord != null && (lord.LordJob is LordJob_DefendPoint || lord.LordJob is LordJob_MechanoidDefendBase) && pawn.CurJobDef != JobDefOf.AttackMelee && pawn.CurJobDef != JobDefOf.AttackStatic)
			{
				return false;
			}
			CompCanBeDormant comp = pawn.GetComp<CompCanBeDormant>();
			if (comp != null && !comp.Awake)
			{
				return false;
			}
		}
		return GenHostility.IsActiveThreatToPlayer(t);
	}
}
