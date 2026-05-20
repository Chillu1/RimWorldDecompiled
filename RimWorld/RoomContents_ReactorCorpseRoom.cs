using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomContents_ReactorCorpseRoom : RoomContents_DeadBody
{
	protected override ThingDef KillerThing => null;

	protected override DamageDef DamageType => DamageDefOf.Bullet;

	protected override Tool ToolUsed => null;

	protected override FloatRange CorpseAgeDaysRange => new FloatRange(300f, 900f);

	protected override IntRange SurvivalPacksCountRange => IntRange.Zero;

	protected override IntRange CorpseRange => new IntRange(4, 6);

	protected override bool AllHaveSameDeathAge => true;

	protected override IEnumerable<PawnKindDef> GetPossibleKinds()
	{
		yield return PawnKindDefOf.Pirate;
	}
}
