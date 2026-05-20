using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatPart_EnvironmentalEffects : StatPart
	{
		private float factorOffsetUnroofed;

		private float factorOffsetOutdoors;

		private float protectedByEdificeFactor;

		public override void TransformValue(StatRequest req, ref float val)
		{
			float num = 0f;
			if (ActiveFor(req.Thing))
			{
				Thing thing = req.Thing;
				if (!thing.Position.Roofed(thing.Map))
				{
					num += factorOffsetUnroofed;
				}
				Room room = thing.GetRoom();
				if (room != null && room.UsesOutdoorTemperature)
				{
					num += factorOffsetOutdoors;
				}
				TerrainDef terrain = thing.Position.GetTerrain(thing.Map);
				if (terrain != null && terrain.extraDeteriorationFactor != 0f)
				{
					num += terrain.extraDeteriorationFactor;
				}
				if (!thing.Position.Roofed(thing.Map))
				{
					num *= Mathf.Lerp(1f, 5f, thing.Map.weatherManager.RainRate);
				}
				if (SteadyEnvironmentEffects.ProtectedByEdifice(thing.Position, thing.Map))
				{
					num *= protectedByEdificeFactor;
				}
				val *= num;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (!ActiveFor(req.Thing))
			{
				return null;
			}
			Thing thing = req.Thing;
			StringBuilder stringBuilder = new StringBuilder();
			if (!thing.Position.Roofed(thing.Map))
			{
				stringBuilder.AppendLine("DeterioratingUnroofed".Translate().CapitalizeFirst() + (": +" + factorOffsetUnroofed.ToStringPercent()));
			}
			Room room = thing.GetRoom();
			if (room != null && room.UsesOutdoorTemperature)
			{
				stringBuilder.AppendLine("DeterioratingOutdoors".Translate().CapitalizeFirst() + (": +" + factorOffsetOutdoors.ToStringPercent()));
			}
			TerrainDef terrain = thing.Position.GetTerrain(thing.Map);
			if (terrain != null && terrain.extraDeteriorationFactor != 0f)
			{
				stringBuilder.AppendLine(terrain.LabelCap + (": +" + terrain.extraDeteriorationFactor.ToStringPercent()));
			}
			if (!thing.Position.Roofed(thing.Map) && thing.Map.weatherManager.RainRate > 0f)
			{
				stringBuilder.AppendLine("DeterioratingRaining".Translate().CapitalizeFirst() + (": x" + Mathf.Lerp(1f, 5f, thing.Map.weatherManager.RainRate).ToStringPercent()));
			}
			if (SteadyEnvironmentEffects.ProtectedByEdifice(thing.Position, thing.Map))
			{
				stringBuilder.AppendLine("Protected".Translate().CapitalizeFirst() + (": x" + protectedByEdificeFactor.ToStringPercent()));
			}
			return stringBuilder.ToString();
		}

		private bool ActiveFor(Thing t)
		{
			if (t != null && t.Spawned)
			{
				return t.def.deteriorateFromEnvironmentalEffects;
			}
			return false;
		}
	}
}
