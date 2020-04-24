using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GenerateNonBuildableMonumentRequiredResources : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> addToList;

		public SlateRef<MonumentMarker> monumentMarker;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			MonumentMarker value = monumentMarker.GetValue(slate);
			for (int i = 0; i < value.sketch.Things.Count; i++)
			{
				ThingDef def = value.sketch.Things[i].def;
				ThingDef stuff = value.sketch.Things[i].stuff;
				if (def.category == ThingCategory.Building && !def.BuildableByPlayer)
				{
					MinifiedThing obj = ThingMaker.MakeThing(def, stuff).MakeMinified();
					QuestGenUtility.AddToOrMakeList(QuestGen.slate, addToList.GetValue(slate), obj);
				}
			}
		}
	}
}
