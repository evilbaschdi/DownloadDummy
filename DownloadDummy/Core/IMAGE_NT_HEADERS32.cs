using System.Runtime.InteropServices;

// ReSharper disable All

namespace DownloadDummy.Core
{
    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_NT_HEADERS32
    {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER32 OptionalHeader;
    }
}