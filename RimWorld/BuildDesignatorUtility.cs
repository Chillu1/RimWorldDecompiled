using Verse;

namespace RimWorld;

public static class BuildDesignatorUtility
{
	public static void TryDrawPowerGridAndAnticipatedConnection(BuildableDef def, Rot4 rotation)
	{
		if (!(def is ThingDef thingDef) || (!thingDef.EverTransmitsPower && !thingDef.ConnectToPower))
		{
			return;
		}
		OverlayDrawHandler.DrawPowerGridOverlayThisFrame();
		if (!thingDef.ConnectToPower)
		{
			return;
		}
		IntVec3 intVec = UI.MouseCell();
		IntVec3 intVec2 = intVec;
		if (thingDef.building.isAttachment)
		{
			Thing wallAttachedTo = GenConstruct.GetWallAttachedTo(intVec, rotation, Find.CurrentMap);
			if (wallAttachedTo != null)
			{
				intVec2 = wallAttachedTo.Position;
			}
		}
		CompPower compPower = PowerConnectionMaker.BestTransmitterForConnector(intVec2, Find.CurrentMap);
		if (compPower != null && !compPower.parent.Position.Fogged(compPower.parent.Map))
		{
			PowerNetGraphics.RenderAnticipatedWirePieceConnecting(intVec2, rotation, def.Size, compPower.parent);
		}
	}
}
