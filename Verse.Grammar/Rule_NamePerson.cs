using RimWorld;

namespace Verse.Grammar
{
	public class Rule_NamePerson : Rule
	{
		public Gender gender;

		public override float BaseSelectionWeight => 1f;

		public override Rule DeepCopy()
		{
			Rule_NamePerson obj = (Rule_NamePerson)base.DeepCopy();
			obj.gender = gender;
			return obj;
		}

		public override string Generate()
		{
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(PawnNameCategory.HumanStandard);
			Gender gender = this.gender;
			if (gender == Gender.None)
			{
				gender = ((Rand.Value < 0.5f) ? Gender.Male : Gender.Female);
			}
			return nameBank.GetName(PawnNameSlot.First, gender, checkIfAlreadyUsed: false);
		}

		public override string ToString()
		{
			return keyword + "->(personname)";
		}
	}
}
