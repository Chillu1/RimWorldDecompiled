using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Health : ITab
	{
		private const int HideBloodLossTicksThreshold = 60000;

		public const float Width = 630f;

		private Pawn PawnForHealth
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				return (base.SelThing as Corpse)?.InnerPawn;
			}
		}

		public ITab_Pawn_Health()
		{
			size = new Vector2(630f, 430f);
			labelKey = "TabHealth";
			tutorTag = "Health";
		}

		protected override void FillTab()
		{
			Pawn pawnForHealth = PawnForHealth;
			if (pawnForHealth == null)
			{
				Log.Error("Health tab found no selected pawn to display.");
				return;
			}
			Corpse corpse = base.SelThing as Corpse;
			bool showBloodLoss = corpse == null || corpse.Age < 60000;
			HealthCardUtility.DrawPawnHealthCard(new Rect(0f, 20f, size.x, size.y - 20f), pawnForHealth, ShouldAllowOperations(), showBloodLoss, base.SelThing);
		}

		private bool ShouldAllowOperations()
		{
			Pawn pawnForHealth = PawnForHealth;
			if (pawnForHealth.Dead)
			{
				return false;
			}
			if (!base.SelThing.def.AllRecipes.Any((RecipeDef x) => x.AvailableNow))
			{
				return false;
			}
			if (pawnForHealth.Faction == Faction.OfPlayer)
			{
				return true;
			}
			if (pawnForHealth.IsPrisonerOfColony || (pawnForHealth.HostFaction == Faction.OfPlayer && !pawnForHealth.health.capacities.CapableOf(PawnCapacityDefOf.Moving)))
			{
				return true;
			}
			if (pawnForHealth.RaceProps.IsFlesh && pawnForHealth.Faction != null && pawnForHealth.Faction.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			if (!pawnForHealth.RaceProps.Humanlike && pawnForHealth.Downed)
			{
				return true;
			}
			return false;
		}
	}
}
