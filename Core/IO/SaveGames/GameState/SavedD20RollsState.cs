using System.IO;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedD20RollsState
    {
        public int NextRollId { get; set; }

        [TempleDllLocation(0x100471e0)]
        public static SavedD20RollsState Read(BinaryReader reader)
        {
            var result = new SavedD20RollsState();
            result.NextRollId = reader.ReadInt32();
            var unusedId = reader.ReadInt32();
            if (unusedId != 1)
            {
                throw new CorruptSaveException("Unused ID is expected to be 1, but was: " + unusedId);
            }

            return result;
        }
    }
}