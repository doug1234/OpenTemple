using System;

namespace SpicyTemple.Core.TigSubsystems
{
    public class TigSound
    {

        public void MssPlaySound(int soundId)
        {
            // TODO
            Console.WriteLine("PLAY SOUND " + soundId);
        }

        [TempleDllLocation(0x101E4360)]
        public void ProcessEvents()
        {
            // TODO SOUND
        }

        [TempleDllLocation(0x101e3f30)]
        public void SetReverb(int roomType, int reverbDry, int reverbWet)
        {
            // TODO SOUND
        }
    }
}