using Verse;

namespace RimWorld
{
	public class CompBiocodable : ThingComp
	{
		protected bool biocoded;

		protected string codedPawnLabel;

		protected Pawn codedPawn;

		public bool Biocoded => biocoded;

		public Pawn CodedPawn => codedPawn;

		public string CodedPawnLabel => codedPawnLabel;

		public void CodeFor(Pawn p)
		{
			biocoded = true;
			codedPawn = p;
			codedPawnLabel = p.Name.ToStringFull;
		}

		public override string TransformLabel(string label)
		{
			if (!biocoded)
			{
				return label;
			}
			return "Biocoded".Translate(label, parent.def).Resolve();
		}

		public override string CompInspectStringExtra()
		{
			if (!biocoded)
			{
				return string.Empty;
			}
			return "CodedFor".Translate(codedPawnLabel.ApplyTag(TagType.Name)).Resolve();
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref biocoded, "biocoded", defaultValue: false);
			Scribe_Values.Look(ref codedPawnLabel, "biocodedPawnLabel");
			Scribe_References.Look(ref codedPawn, "codedPawn", saveDestroyedThings: true);
		}
	}
}
