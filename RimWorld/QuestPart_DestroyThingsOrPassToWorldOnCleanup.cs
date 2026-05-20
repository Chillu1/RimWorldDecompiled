using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class QuestPart_DestroyThingsOrPassToWorldOnCleanup : QuestPart
{
	public List<Thing> things = new List<Thing>();

	public bool questLookTargets = true;

	public override IEnumerable<GlobalTargetInfo> QuestLookTargets
	{
		get
		{
			if (questLookTargets)
			{
				for (int i = 0; i < things.Count; i++)
				{
					yield return things[i];
				}
			}
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		QuestPart_DestroyThingsOrPassToWorld.Destroy(things);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Collections.Look(ref things, "things", LookMode.Reference);
		Scribe_Values.Look(ref questLookTargets, "questLookTargets", defaultValue: true);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			things.RemoveAll((Thing x) => x == null);
		}
	}
}
