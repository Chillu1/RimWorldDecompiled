using Verse;

namespace RimWorld;

public class GameCondition_DeathPall : GameCondition_ForceWeather
{
	private static readonly IntRange ResurrectIntervalRange = new IntRange(600, 1800);

	private const int ResurrectCheckInterval = 60;

	private const int ResurrectCooldown = 15000;

	private const int ResurrectMessageCooldown = 60000;

	private int nextResurrectTick;

	private int lastResurrectMessageTick = -9999;

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref nextResurrectTick, "nextResurrectTick", 0);
		Scribe_Values.Look(ref lastResurrectMessageTick, "lastResurrectMessageTick", -9999);
	}

	public override void Init()
	{
		if (!ModLister.CheckAnomaly("Death pall"))
		{
			End();
			return;
		}
		base.Init();
		nextResurrectTick = Find.TickManager.TicksGame + ResurrectIntervalRange.RandomInRange;
	}

	public override void GameConditionTick()
	{
		if (Find.TickManager.TicksGame < nextResurrectTick || Find.TickManager.TicksGame % 60 != 0)
		{
			return;
		}
		foreach (Map affectedMap in base.AffectedMaps)
		{
			foreach (Thing item in affectedMap.listerThings.ThingsInGroup(ThingRequestGroup.Corpse))
			{
				if (item is Corpse corpse && MutantUtility.CanResurrectAsShambler(corpse) && corpse.Age >= 15000)
				{
					Pawn pawn = ResurrectPawn(corpse);
					if (!pawn.Position.Fogged(affectedMap))
					{
						TryDoMessage(pawn);
					}
					nextResurrectTick = Find.TickManager.TicksGame + ResurrectIntervalRange.RandomInRange;
					return;
				}
			}
		}
	}

	private Pawn ResurrectPawn(Corpse corpse)
	{
		Pawn innerPawn = corpse.InnerPawn;
		MutantUtility.ResurrectAsShambler(innerPawn, 60000);
		return innerPawn;
	}

	public override void End()
	{
		Find.LetterStack.ReceiveLetter("LetterLabelDeathPallEnded".Translate(), "LetterDeathPallEnded".Translate(), LetterDefOf.NeutralEvent);
		base.End();
		base.SingleMap.weatherDecider.StartNextWeather();
	}

	private void TryDoMessage(LookTargets target)
	{
		if (Find.TickManager.TicksGame - lastResurrectMessageTick >= 60000)
		{
			Messages.Message("DeathPallResurrectedMessage".Translate(), target, MessageTypeDefOf.NegativeEvent, historical: false);
			lastResurrectMessageTick = Find.TickManager.TicksGame;
		}
	}
}
