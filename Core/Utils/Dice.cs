using System;
using System.Buffers.Text;
using System.Diagnostics;
using JetBrains.Annotations;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.Utils
{


    public struct Dice
    {

        public static readonly Dice D20 = new Dice(1, 20);

        public static readonly Dice D100 = new Dice(1, 100);

        public int Count { get; }

        public int Sides { get; }

        public int Modifier { get; }

        public bool IsValid => Count >= 1 && Sides >= 1;

        public Dice(int count, int sides, int modifier = 0)
        {
            Count = count;
            Sides = sides;
            Modifier = modifier;
        }

        /// <summary>
        /// Performs a dice roll with the given parameters and returns
        /// the result.
        /// </summary>
        public static int Roll(int count, int sides, int modifier = 0)
        {
            var result = modifier;
            for (var i = 0; i < count; i++)
            {
                result += GameSystems.Random.GetInt(1, sides);
            }

            return result;
        }

        [Pure]
        public int Roll() => Roll(Count, Sides, Modifier);

        /*
            Parses a dice string (i.e. 2d5+1) into its components and returns true
            on success. The modifier part is optional and can be negative.
        */
        [TempleDllLocation(0x10038ba0)]
        public static bool TryParse(ReadOnlySpan<byte> diceStr, out Dice dice)
        {
            var idxOfSep = diceStr.IndexOf((byte) 'd');
            if (idxOfSep == -1)
            {
                dice = default;
                return false;
            }

            var countSpan = diceStr.Slice(0, idxOfSep);
            if (!Utf8Parser.TryParse(countSpan, out int count, out _))
            {
                dice = default;
                return false;
            }

            var modifier = 0;
            var idxOfModifierSign = diceStr.IndexOf((byte) '-');
            ReadOnlySpan<byte> sidesSpan;
            if (idxOfModifierSign != -1)
            {
                if (idxOfModifierSign < idxOfSep)
                {
                    dice = default;
                    return false;
                }

                // Negative modifier
                sidesSpan = diceStr.Slice(idxOfSep + 1, idxOfModifierSign - idxOfSep - 1);
                var modifierSpan = diceStr.Slice(idxOfModifierSign);
                if (!Utf8Parser.TryParse(modifierSpan, out modifier, out _))
                {
                    dice = default;
                    return false;
                }
            }
            else
            {
                idxOfModifierSign = diceStr.IndexOf((byte) '+');
                if (idxOfModifierSign != -1)
                {
                    if (idxOfModifierSign < idxOfSep)
                    {
                        dice = default;
                        return false;
                    }

                    // Positive modifier
                    sidesSpan = diceStr.Slice(idxOfSep + 1, idxOfModifierSign - idxOfSep - 1);
                    var modifierSpan = diceStr.Slice(idxOfModifierSign + 1);
                    if (!Utf8Parser.TryParse(modifierSpan, out modifier, out _))
                    {
                        dice = default;
                        return false;
                    }
                }
                else
                {
                    sidesSpan = diceStr.Slice(idxOfSep + 1);
                }
            }

            if (!Utf8Parser.TryParse(sidesSpan, out int sides, out _))
            {
                dice = default;
                return false;
            }

            dice = new Dice(count, sides, modifier);
            return true;
        }

        // Convert to a packed ToEE dice
        [TempleDllLocation(0x10038c50)]
        public int ToPacked()
        {
            uint result = (uint) ((Count & 0x7F) | ((Sides & 0x7F) << 7));
            if (Modifier < 0)
            {
                result |= (uint) ((-Modifier) & 0x7F) << 14;
                result |= 0x80000000;
            }
            else
            {
                result |= (uint) (Modifier & 0x7F) << 14;
            }

            return unchecked((int) result);
        }

        [TempleDllLocation(0x10038c40)]
        [TempleDllLocation(0x10038c30)]
        [TempleDllLocation(0x10038c90)]
        public static Dice Unpack(uint packed)
        {
            bool modNegative = (packed & 0x80000000) != 0;
            int mod = (int) ((packed >> 14) & 0x7F);
            int count = (int) (packed & 0x7F);
            int sides = (int) ((packed >> 7) & 0x7F);
            if (modNegative)
            {
                mod = -mod;
            }

            return new Dice(count, sides, mod);
        }

        [TempleDllLocation(0x10038d50)]
        public float ExpectedValue => (Sides + 1.0f) * Sides / (2.0f * Sides) * Count + Modifier;

        public int MaximumValue => Count + Sides + Modifier;

    }
}