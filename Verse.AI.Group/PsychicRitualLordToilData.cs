namespace Verse.AI.Group;

public class PsychicRitualLordToilData : LordToilData
{
	public PsychicRitual psychicRitual;

	public PsychicRitualToil psychicRitualToil;

	public bool done;

	public int iteration;

	public bool playerRitual;

	public bool removeLordOnCancel;

	public PsychicRitualToil CurPsychicRitualToil
	{
		get
		{
			if (psychicRitualToil is PsychicRitualGraph psychicRitualGraph)
			{
				return psychicRitualGraph.CurrentToil;
			}
			return psychicRitualToil;
		}
	}

	public override void ExposeData()
	{
		Scribe_Deep.Look(ref psychicRitualToil, "psychicRitualToil");
		Scribe_Deep.Look(ref psychicRitual, "psychicRitual");
		Scribe_Values.Look(ref done, "done", defaultValue: false);
		Scribe_Values.Look(ref iteration, "iteration", 0);
		Scribe_Values.Look(ref playerRitual, "playerRitual", defaultValue: false);
		Scribe_Values.Look(ref removeLordOnCancel, "removeLordOnCancel", defaultValue: false);
	}
}
