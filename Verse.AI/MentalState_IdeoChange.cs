using RimWorld;

namespace Verse.AI;

public class MentalState_IdeoChange : MentalState
{
	private Ideo oldIdeo;

	private Ideo newIdeo;

	private Precept_Role oldRole;

	private bool changedIdeo;

	private float newCertainty;

	private const float ConversionCertaintyReduction = 0.5f;

	public override void PreStart()
	{
		base.PreStart();
		oldIdeo = pawn.Ideo;
		oldRole = oldIdeo.GetRole(pawn);
		newIdeo = Find.IdeoManager.IdeosListForReading.RandomElementByWeight((Ideo x) => IdeoUtility.IdeoChangeToWeight(pawn, x));
		if (pawn.ideo.IdeoConversionAttempt(0.5f, newIdeo, applyCertaintyFactor: false))
		{
			changedIdeo = true;
		}
		newCertainty = pawn.ideo.Certainty;
	}

	public override TaggedString GetBeginLetterText()
	{
		TaggedString result = def.beginLetter.Formatted(pawn.LabelShort, pawn.Named("PAWN"), pawn.Ideo.Named("NEWIDEO"), oldIdeo.Named("OLDIDEO")).CapitalizeFirst() + "\n\n";
		if (changedIdeo)
		{
			result += "LetterIdeoChangeConverted".Translate(pawn.Named("PAWN"), newIdeo.Named("NEWIDEO"), oldIdeo.Named("OLDIDEO")).CapitalizeFirst();
		}
		else
		{
			result += "LetterIdeoChangeNotConverted".Translate(pawn.Named("PAWN"), oldIdeo.Named("OLDIDEO"), newCertainty.ToStringPercent().Named("NEWCERTAINTY")).CapitalizeFirst();
		}
		MentalStateDef wanderToOwnRoomStateOrFallback = MentalStateUtility.GetWanderToOwnRoomStateOrFallback(pawn);
		if (wanderToOwnRoomStateOrFallback == MentalStateDefOf.Wander_OwnRoom)
		{
			result += "\n\n" + "LetterIdeoChangedWanderOwnRoom".Translate(pawn.Named("PAWN"));
		}
		else if (wanderToOwnRoomStateOrFallback == MentalStateDefOf.Wander_Sad)
		{
			result += "\n\n" + "LetterIdeoChangedSadWander".Translate(pawn.Named("PAWN"));
		}
		if (changedIdeo && oldRole != null)
		{
			result += "\n\n" + "LetterRoleLostLetterIdeoChangedPostfix".Translate(pawn.Named("PAWN"), oldRole.Named("ROLE"), oldIdeo.Named("OLDIDEO"));
		}
		return result;
	}

	public override void PostStart(string reason)
	{
		base.PostStart(reason);
		MentalStateUtility.TryTransitionToWanderOwnRoom(this);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref oldIdeo, "oldIdeo");
		Scribe_References.Look(ref newIdeo, "newIdeo");
		Scribe_References.Look(ref oldRole, "oldRole");
		Scribe_Values.Look(ref changedIdeo, "changedIdeo", defaultValue: false);
		Scribe_Values.Look(ref newCertainty, "newCertainty", 0f);
	}
}
