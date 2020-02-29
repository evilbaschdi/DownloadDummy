using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DownloadDummy.Core
{
    static class ExeChecker
    {
        const ushort IMAGE_DOS_SIGNATURE = 0x5A4D; // MZ
        const uint IMAGE_NT_SIGNATURE = 0x00004550; // PE00

        const ushort IMAGE_FILE_MACHINE_I386 = 0x014C; // Intel 386
        const ushort IMAGE_FILE_MACHINE_IA64 = 0x0200; // Intel 64
        const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664; // AMD64

        const ushort IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10B; // PE32
        const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20B; // PE32+

        const ushort IMAGE_FILE_DLL = 0x2000;

        public static bool IsValidExe(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }

            try
            {
                using (var stream = File.OpenRead(fileName))
                {
                    IMAGE_DOS_HEADER dosHeader = GetDosHeader(stream);
                    if (dosHeader.e_magic != IMAGE_DOS_SIGNATURE)
                    {
                        return false;
                    }

                    IMAGE_NT_HEADERS_COMMON ntHeader = GetCommonNtHeader(stream, dosHeader);
                    if (ntHeader.Signature != IMAGE_NT_SIGNATURE)
                    {
                        return false;
                    }

                    if ((ntHeader.FileHeader.Characteristics & IMAGE_FILE_DLL) != 0)
                    {
                        return false;
                    }

                    switch (ntHeader.FileHeader.Machine)
                    {
                        case IMAGE_FILE_MACHINE_I386:
                            return IsValidExe32(GetNtHeader32(stream, dosHeader));

                        case IMAGE_FILE_MACHINE_IA64:
                        case IMAGE_FILE_MACHINE_AMD64:
                            return IsValidExe64(GetNtHeader64(stream, dosHeader));
                    }
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        static bool IsValidExe32(IMAGE_NT_HEADERS32 ntHeader)
        {
            return ntHeader.OptionalHeader.Magic == IMAGE_NT_OPTIONAL_HDR32_MAGIC;
        }

        static bool IsValidExe64(IMAGE_NT_HEADERS64 ntHeader)
        {
            return ntHeader.OptionalHeader.Magic == IMAGE_NT_OPTIONAL_HDR64_MAGIC;
        }

        static IMAGE_DOS_HEADER GetDosHeader(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_DOS_HEADER>(stream);
        }

        static IMAGE_NT_HEADERS_COMMON GetCommonNtHeader(Stream stream, IMAGE_DOS_HEADER dosHeader)
        {
            stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_NT_HEADERS_COMMON>(stream);
        }

        static IMAGE_NT_HEADERS32 GetNtHeader32(Stream stream, IMAGE_DOS_HEADER dosHeader)
        {
            stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_NT_HEADERS32>(stream);
        }

        static IMAGE_NT_HEADERS64 GetNtHeader64(Stream stream, IMAGE_DOS_HEADER dosHeader)
        {
            stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_NT_HEADERS64>(stream);
        }

        static T ReadStructFromStream<T>(Stream stream)
        {
            int structSize = Marshal.SizeOf(typeof(T));
            IntPtr memory = IntPtr.Zero;

            try
            {
                memory = Marshal.AllocCoTaskMem(structSize);
                if (memory == IntPtr.Zero)
                {
                    throw new InvalidOperationException();
                }

                byte[] buffer = new byte[structSize];
                int bytesRead = stream.Read(buffer, 0, structSize);
                if (bytesRead != structSize)
                {
                    throw new InvalidOperationException();
                }

                Marshal.Copy(buffer, 0, memory, structSize);

                return (T) Marshal.PtrToStructure(memory, typeof(T));
            }
            finally
            {
                if (memory != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(memory);
                }
            }
        }
    }
}