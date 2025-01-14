using System;

namespace OpenTemple.Core.GFX;

public class GfxException : Exception
{
    public GfxException(string message) : base(message)
    {
    }

    public GfxException(string message, Exception innerException) : base(message, innerException)
    {
    }
}