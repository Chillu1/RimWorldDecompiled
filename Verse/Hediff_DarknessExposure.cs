using System.Collections.Generic;
using RimWorld;
using Verse.Sound;

namespace Verse;

public class Hediff_DarknessExposure : Hediff
{
	private int nextDamageTick = -1;

	private int lastNotifyTick = -1;

	private static readonly IntRange DamageIntervalTicks_Fresh = new IntRange(480, 720);

	private static readonly IntRange DamageIntervalTicks_NonFresh = new IntRange(180, 300);

	private static readonly FloatRange DamageRange = new FloatRange(2.5f, 5.5f);

	private const int TicksBetweenWarnings = 5000;

	private static List<DamageDef> possibleDamageDefs;

	public override bool ShouldRemove
	{
		get
		{
			if (pawn.Spawned && !pawn.Downed)
			{
				return !GameCondition_UnnaturalDarkness.InUnnaturalDarkness(pawn);
			}
			return true;
		}
	}

	private DamageDef RandomDamageDef
	{
		get
		{
			if (possibleDamageDefs == null)
			{
				possibleDamageDefs = new List<DamageDef>
				{
					DamageDefOf.Scratch,
					DamageDefOf.Bite,
					DamageDefOf.Cut
				};
			}
			return possibleDamageDefs.RandomElement();
		}
	}

	public override void PostAdd(DamageInfo? dinfo)
	{
		if (!ModLister.CheckAnomaly("Darkness exposure"))
		{
			pawn.health.RemoveHediff(this);
		}
		else
		{
			base.PostAdd(dinfo);
		}
	}

	public override void TickInterval(int delta)
	{
		base.TickInterval(delta);
		if (nextDamageTick < 0)
		{
			nextDamageTick = Find.TickManager.TicksGame + DamageIntervalTicks_Fresh.RandomInRange;
		}
		if (Find.TickManager.TicksGame >= nextDamageTick)
		{
			SoundDefOf.DarknessDamage.PlayOneShot(pawn);
			BattleLogEntry_DamageTaken battleLogEntry_DamageTaken = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_UnnaturalDarkness);
			Find.BattleLog.Add(battleLogEntry_DamageTaken);
			pawn.TakeDamage(new DamageInfo(RandomDamageDef, DamageRange.RandomInRange)).AssociateWithLog(battleLogEntry_DamageTaken);
			nextDamageTick = Find.TickManager.TicksGame + DamageIntervalTicks_NonFresh.RandomInRange;
			CheckNotifyPlayer();
		}
	}

	private void CheckNotifyPlayer()
	{
		if ((PawnUtility.ShouldSendNotificationAbout(pawn) || pawn.IsColonySubhumanPlayerControlled) && lastNotifyTick + 5000 < Find.TickManager.TicksGame)
		{
			GameCondition_UnnaturalDarkness activeCondition = pawn.MapHeld.gameConditionManager.GetActiveCondition<GameCondition_UnnaturalDarkness>();
			if (activeCondition != null && !activeCondition.anyColonistAttacked)
			{
				Find.LetterStack.ReceiveLetter("PawnAttackedInDarknessLabel".Translate(pawn), "PawnAttackedInDarknessText".Translate(pawn), LetterDefOf.ThreatSmall, pawn);
				activeCondition.anyColonistAttacked = true;
			}
			else
			{
				Messages.Message("MessagePawnAttackedInDarkness".Translate(pawn), pawn, MessageTypeDefOf.ThreatSmall, historical: false);
			}
			lastNotifyTick = Find.TickManager.TicksGame;
		}
	}

	public override bool TryMergeWith(Hediff other)
	{
		return false;
	}

	public override void Notify_Downed()
	{
		nextDamageTick = -1;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref nextDamageTick, "nextDamageTick", -1);
		Scribe_Values.Look(ref lastNotifyTick, "lastNotifyTick", -1);
	}
}
