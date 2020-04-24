using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GenerateWorldObject : QuestNode
	{
		public SlateRef<WorldObjectDef> def;

		public SlateRef<int> tile;

		public SlateRef<Faction> faction;

		[NoTranslate]
		public SlateRef<string> storeAs;

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			WorldObject worldObject = WorldObjectMaker.MakeWorldObject(def.GetValue(slate));
			worldObject.Tile = tile.GetValue(slate);
			if (faction.GetValue(slate) != null)
			{
				worldObject.SetFaction(faction.GetValue(slate));
			}
			if (storeAs.GetValue(slate) != null)
			{
				QuestGen.slate.Set(storeAs.GetValue(slate), worldObject);
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}
	}
}
