using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class CompIngredients : ThingComp
	{
		public List<ThingDef> ingredients = new List<ThingDef>();

		private const int MaxNumIngredients = 3;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Def);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && ingredients == null)
			{
				ingredients = new List<ThingDef>();
			}
		}

		public void RegisterIngredient(ThingDef def)
		{
			if (!ingredients.Contains(def))
			{
				ingredients.Add(def);
			}
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			if (piece != parent)
			{
				CompIngredients compIngredients = piece.TryGetComp<CompIngredients>();
				for (int i = 0; i < ingredients.Count; i++)
				{
					compIngredients.ingredients.Add(ingredients[i]);
				}
			}
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			base.PreAbsorbStack(otherStack, count);
			List<ThingDef> list = otherStack.TryGetComp<CompIngredients>().ingredients;
			for (int i = 0; i < list.Count; i++)
			{
				if (!ingredients.Contains(list[i]))
				{
					ingredients.Add(list[i]);
				}
			}
			if (ingredients.Count > 3)
			{
				ingredients.Shuffle();
				while (ingredients.Count > 3)
				{
					ingredients.Remove(ingredients[ingredients.Count - 1]);
				}
			}
		}

		public override string CompInspectStringExtra()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (ingredients.Count > 0)
			{
				stringBuilder.Append("Ingredients".Translate() + ": ");
				for (int i = 0; i < ingredients.Count; i++)
				{
					stringBuilder.Append((i == 0) ? ingredients[i].LabelCap.Resolve() : ingredients[i].label);
					if (i < ingredients.Count - 1)
					{
						stringBuilder.Append(", ");
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
