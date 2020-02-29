using System.Runtime.InteropServices;

namespace DownloadDummy.Core
{
    [StructLayout(LayoutKind.Sequential)]
    struct IMAGE_NT_HEADERS_COMMON
    {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
    }
}