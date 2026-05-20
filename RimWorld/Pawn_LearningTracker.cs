using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class Pawn_LearningTracker : IExposable
{
	private const int MaxActiveLearningDesires = 2;

	private const int MinLearningDesireIntervalTicks = 20000;

	private const int MaxLearningDesireIntervalTicks = 40000;

	private Pawn pawn;

	private List<LearningDesireDef> active = new List<LearningDesireDef>();

	private int newLearningDesireTicksLeft;

	public Pawn Pawn => pawn;

	public List<LearningDesireDef> ActiveLearningDesires => active;

	public Pawn_LearningTracker()
	{
	}

	public Pawn_LearningTracker(Pawn pawn)
	{
		this.pawn = pawn;
	}

	public void AddNewLearningDesire()
	{
		LearningDesireDef item = DefDatabase<LearningDesireDef>.AllDefsListForReading.Where((LearningDesireDef ld) => !active.Contains(ld) && ld.Worker.CanGiveDesire).RandomElementByWeight((LearningDesireDef ld) => ld.selectionWeight);
		if (active.Count >= 2)
		{
			active.RemoveAt(0);
		}
		active.Add(item);
	}

	public void LearningTickInterval(int delta)
	{
		newLearningDesireTicksLeft -= delta;
		if (newLearningDesireTicksLeft <= 0)
		{
			AddNewLearningDesire();
			newLearningDesireTicksLeft = Rand.Range(20000, 40000);
		}
	}

	public void Debug_SetLearningDesire(LearningDesireDef desire)
	{
		active.Clear();
		active.Add(desire);
	}

	public void ExposeData()
	{
		Scribe_Values.Look(ref newLearningDesireTicksLeft, "newLearningDesireTicksLeft", 0);
		Scribe_Collections.Look(ref active, "active", LookMode.Def);
	}
}
