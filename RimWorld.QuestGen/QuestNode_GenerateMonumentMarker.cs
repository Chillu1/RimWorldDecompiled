using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GenerateMonumentMarker : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeAs;

		public SlateRef<Sketch> sketch;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			MonumentMarker monumentMarker = (MonumentMarker)ThingMaker.MakeThing(ThingDefOf.MonumentMarker);
			monumentMarker.sketch = sketch.GetValue(slate);
			slate.Set(storeAs.GetValue(slate), monumentMarker);
		}
	}
}
