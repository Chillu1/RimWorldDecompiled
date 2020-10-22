using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld
{
	public class QuestPart_RequirementsToAcceptNoDanger : QuestPart_RequirementsToAccept
	{
		public Map map;

		public Faction dangerTo;

		public override IEnumerable<GlobalTargetInfo> Culprits
		{
			get
			{
				if (GenHostility.AnyHostileActiveThreatTo(map, dangerTo, out var threat, countDormantPawnsAsHostile: true))
				{
					yield return (Thing)threat;
				}
			}
		}

		public override AcceptanceReport CanAccept()
		{
			if (map != null && GenHostility.AnyHostileActiveThreatTo(map, dangerTo, countDormantPawnsAsHostile: true))
			{
				return new AcceptanceReport("QuestRequiresNoDangerOnMap".Translate());
			}
			return true;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref map, "map");
			Scribe_References.Look(ref dangerTo, "dangerTo");
		}
	}
}
