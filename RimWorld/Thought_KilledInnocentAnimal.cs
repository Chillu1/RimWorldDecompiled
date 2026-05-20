using Verse;

namespace RimWorld;

public class Thought_KilledInnocentAnimal : Thought_Memory
{
	public PawnKindDef animal;

	public Gender gender;

	public bool plural;

	public bool otherAnimals;

	public override string LabelCap
	{
		get
		{
			string text = GenLabel.BestKindLabel(animal, gender, plural);
			if (otherAnimals)
			{
				text += ", " + "Etc".Translate();
			}
			return base.CurStage.label.Formatted(text).CapitalizeFirst();
		}
	}

	public void SetAnimal(Pawn animal)
	{
		this.animal = animal.kindDef;
		gender = animal.gender;
	}

	public override void Notify_NewThoughtInGroupAdded(Thought_Memory memory)
	{
		base.Notify_NewThoughtInGroupAdded(memory);
		if (memory is Thought_KilledInnocentAnimal thought_KilledInnocentAnimal)
		{
			if (thought_KilledInnocentAnimal.animal == animal)
			{
				plural = true;
			}
			else
			{
				otherAnimals = true;
			}
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Defs.Look(ref animal, "animal");
		Scribe_Values.Look(ref gender, "gender", Gender.None);
		Scribe_Values.Look(ref plural, "plural", defaultValue: false);
		Scribe_Values.Look(ref otherAnimals, "otherAnimals", defaultValue: false);
	}
}
