using System.Runtime.InteropServices;

namespace DownloadDummy.Core
{
    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_NT_HEADERS64
    {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
    }
}