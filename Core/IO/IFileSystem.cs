using System.Buffers;
using System.Collections.Generic;
using System.IO;

namespace SpicyTemple.Core.IO
{
    public interface IFileSystem
    {
        bool FileExists(string path);

        bool DirectoryExists(string path);

        string ReadTextFile(string path);

        ISet<string> ListDirectory(string path);

        BinaryReader OpenBinaryReader(string path);

        byte[] ReadBinaryFile(string path);

        IMemoryOwner<byte> ReadFile(string path);
    }

}