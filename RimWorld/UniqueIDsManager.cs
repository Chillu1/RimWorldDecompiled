using Verse;

namespace RimWorld
{
	public class UniqueIDsManager : IExposable
	{
		private int nextThingID;

		private int nextBillID;

		private int nextFactionID;

		private int nextLordID;

		private int nextTaleID;

		private int nextPassingShipID;

		private int nextWorldObjectID;

		private int nextMapID;

		private int nextCaravanID;

		private int nextAreaID;

		private int nextTransporterGroupID;

		private int nextAncientCryptosleepCasketGroupID;

		private int nextJobID;

		private int nextSignalTagID;

		private int nextWorldFeatureID;

		private int nextHediffID;

		private int nextBattleID;

		private int nextLogID;

		private int nextLetterID;

		private int nextArchivedDialogID;

		private int nextMessageID;

		private int nextZoneID;

		private int nextQuestID;

		private int nextGameConditionID;

		public int GetNextThingID()
		{
			return GetNextID(ref nextThingID);
		}

		public int GetNextBillID()
		{
			return GetNextID(ref nextBillID);
		}

		public int GetNextFactionID()
		{
			return GetNextID(ref nextFactionID);
		}

		public int GetNextLordID()
		{
			return GetNextID(ref nextLordID);
		}

		public int GetNextTaleID()
		{
			return GetNextID(ref nextTaleID);
		}

		public int GetNextPassingShipID()
		{
			return GetNextID(ref nextPassingShipID);
		}

		public int GetNextWorldObjectID()
		{
			return GetNextID(ref nextWorldObjectID);
		}

		public int GetNextMapID()
		{
			return GetNextID(ref nextMapID);
		}

		public int GetNextCaravanID()
		{
			return GetNextID(ref nextCaravanID);
		}

		public int GetNextAreaID()
		{
			return GetNextID(ref nextAreaID);
		}

		public int GetNextTransporterGroupID()
		{
			return GetNextID(ref nextTransporterGroupID);
		}

		public int GetNextAncientCryptosleepCasketGroupID()
		{
			return GetNextID(ref nextAncientCryptosleepCasketGroupID);
		}

		public int GetNextJobID()
		{
			return GetNextID(ref nextJobID);
		}

		public int GetNextSignalTagID()
		{
			return GetNextID(ref nextSignalTagID);
		}

		public int GetNextWorldFeatureID()
		{
			return GetNextID(ref nextWorldFeatureID);
		}

		public int GetNextHediffID()
		{
			return GetNextID(ref nextHediffID);
		}

		public int GetNextBattleID()
		{
			return GetNextID(ref nextBattleID);
		}

		public int GetNextLogID()
		{
			return GetNextID(ref nextLogID);
		}

		public int GetNextLetterID()
		{
			return GetNextID(ref nextLetterID);
		}

		public int GetNextArchivedDialogID()
		{
			return GetNextID(ref nextArchivedDialogID);
		}

		public int GetNextMessageID()
		{
			return GetNextID(ref nextMessageID);
		}

		public int GetNextZoneID()
		{
			return GetNextID(ref nextZoneID);
		}

		public int GetNextQuestID()
		{
			return GetNextID(ref nextQuestID);
		}

		public int GetNextGameConditionID()
		{
			return GetNextID(ref nextGameConditionID);
		}

		public UniqueIDsManager()
		{
			nextThingID = Rand.Range(0, 1000);
		}

		private static int GetNextID(ref int nextID)
		{
			if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
			{
				Log.Warning("Getting next unique ID during saving or loading. This may cause bugs.");
			}
			int result = nextID;
			nextID++;
			if (nextID == int.MaxValue)
			{
				Log.Warning("Next ID is at max value. Resetting to 0. This may cause bugs.");
				nextID = 0;
			}
			return result;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref nextThingID, "nextThingID", 0);
			Scribe_Values.Look(ref nextBillID, "nextBillID", 0);
			Scribe_Values.Look(ref nextFactionID, "nextFactionID", 0);
			Scribe_Values.Look(ref nextLordID, "nextLordID", 0);
			Scribe_Values.Look(ref nextTaleID, "nextTaleID", 0);
			Scribe_Values.Look(ref nextPassingShipID, "nextPassingShipID", 0);
			Scribe_Values.Look(ref nextWorldObjectID, "nextWorldObjectID", 0);
			Scribe_Values.Look(ref nextMapID, "nextMapID", 0);
			Scribe_Values.Look(ref nextCaravanID, "nextCaravanID", 0);
			Scribe_Values.Look(ref nextAreaID, "nextAreaID", 0);
			Scribe_Values.Look(ref nextTransporterGroupID, "nextTransporterGroupID", 0);
			Scribe_Values.Look(ref nextAncientCryptosleepCasketGroupID, "nextAncientCryptosleepCasketGroupID", 0);
			Scribe_Values.Look(ref nextJobID, "nextJobID", 0);
			Scribe_Values.Look(ref nextSignalTagID, "nextSignalTagID", 0);
			Scribe_Values.Look(ref nextWorldFeatureID, "nextWorldFeatureID", 0);
			Scribe_Values.Look(ref nextHediffID, "nextHediffID", 0);
			Scribe_Values.Look(ref nextBattleID, "nextBattleID", 0);
			Scribe_Values.Look(ref nextLogID, "nextLogID", 0);
			Scribe_Values.Look(ref nextLetterID, "nextLetterID", 0);
			Scribe_Values.Look(ref nextArchivedDialogID, "nextArchivedDialogID", 0);
			Scribe_Values.Look(ref nextMessageID, "nextMessageID", 0);
			Scribe_Values.Look(ref nextZoneID, "nextZoneID", 0);
			Scribe_Values.Look(ref nextQuestID, "nextQuestID", 0);
			Scribe_Values.Look(ref nextGameConditionID, "nextGameConditionID", 0);
		}
	}
}
