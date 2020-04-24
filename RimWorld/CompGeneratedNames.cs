using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class CompGeneratedNames : ThingComp
	{
		private string name;

		public CompProperties_GeneratedName Props => (CompProperties_GeneratedName)props;

		public override string TransformLabel(string label)
		{
			if (parent.GetComp<CompBladelinkWeapon>() != null)
			{
				return name + ", " + label;
			}
			return name + " (" + label + ")";
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			GrammarRequest request = default(GrammarRequest);
			request.Includes.Add(Props.nameMaker);
			name = GenText.CapitalizeAsTitle(GrammarResolver.Resolve("r_weapon_name", request));
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref name, "name");
		}
	}
}
