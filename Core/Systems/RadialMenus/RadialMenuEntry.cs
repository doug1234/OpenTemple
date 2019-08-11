﻿using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using System;

namespace SpicyTemple.Core.Systems.RadialMenus
{
    public struct RadialMenuEntry
    {

        public string text; // Text to display
        public int text2; // string for popup dialog title, so far
        public int textHash; // ELF hash of "text"
        public int fieldc;
        public RadialMenuEntryType type; // May define how the children are ordered (seen 4 been used here)
        public int minArg;
        public int maxArg;
        public int actualArg;
        public D20ActionType d20ActionType;
        public int d20ActionData1;
        public D20CAF d20Caf;
        public D20SpellData d20SpellData;
        public D20DispatcherKey dispKey; // example: DestructionDomainRadialMenu (the only one I've encountered so far), using this for python actions too now
        // TODO public BOOL(__cdecl* callback)(objHndl a1, RadialMenuEntry* entry);
	    public int flags; // see RadialMenuEntryFlags
        public string helpSystemHashkey; // String hash for the help topic associated with this entry
        public int spellIdMaybe; // used for stuff like Break Free / Dismiss Spell, and it also puts the id in the d20ActionData1 field

        [TempleDllLocation(0x100f0af0)]
        public static RadialMenuEntry Create()
        {
            throw new NotImplementedException();
        }

    }
}
