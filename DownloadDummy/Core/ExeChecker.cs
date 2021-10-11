﻿using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DownloadDummy.Core
{
    internal static class ExeChecker
    {
        private const ushort ImageDosSignature = 0x5A4D; // MZ
        private const uint ImageNtSignature = 0x00004550; // PE00

        private const ushort ImageFileMachineI386 = 0x014C; // Intel 386
        private const ushort ImageFileMachineIa64 = 0x0200; // Intel 64
        private const ushort ImageFileMachineAmd64 = 0x8664; // AMD64

        private const ushort ImageNtOptionalHdr32Magic = 0x10B; // PE32
        private const ushort ImageNtOptionalHdr64Magic = 0x20B; // PE32+

        private const ushort ImageFileDll = 0x2000;

        public static bool IsValidExe(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }

            try
            {
                using var stream = File.OpenRead(fileName);
                var dosHeader = GetDosHeader(stream);
                if (dosHeader.e_magic != ImageDosSignature)
                {
                    return false;
                }

                var ntHeader = GetCommonNtHeader(stream, dosHeader);
                if (ntHeader.Signature != ImageNtSignature)
                {
                    return false;
                }

                if ((ntHeader.FileHeader.Characteristics & ImageFileDll) != 0)
                {
                    return false;
                }

                switch (ntHeader.FileHeader.Machine)
                {
                    case ImageFileMachineI386:
                        return IsValidExe32(GetNtHeader32(stream, dosHeader));

                    case ImageFileMachineIa64:
                    case ImageFileMachineAmd64:
                        return IsValidExe64(GetNtHeader64(stream, dosHeader));
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        private static bool IsValidExe32(IMAGE_NT_HEADERS32 ntHeader)
        {
            return ntHeader.OptionalHeader.Magic == ImageNtOptionalHdr32Magic;
        }

        private static bool IsValidExe64(IMAGE_NT_HEADERS64 ntHeader)
        {
            return ntHeader.OptionalHeader.Magic == ImageNtOptionalHdr64Magic;
        }

        private static IMAGE_DOS_HEADER GetDosHeader(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_DOS_HEADER>(stream);
        }

        private static IMAGE_NT_HEADERS_COMMON GetCommonNtHeader(Stream stream, IMAGE_DOS_HEADER dosHeader)
        {
            stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_NT_HEADERS_COMMON>(stream);
        }

        private static IMAGE_NT_HEADERS32 GetNtHeader32(Stream stream, IMAGE_DOS_HEADER dosHeader)
        {
            stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_NT_HEADERS32>(stream);
        }

        private static IMAGE_NT_HEADERS64 GetNtHeader64(Stream stream, IMAGE_DOS_HEADER dosHeader)
        {
            stream.Seek(dosHeader.e_lfanew, SeekOrigin.Begin);
            return ReadStructFromStream<IMAGE_NT_HEADERS64>(stream);
        }

        private static T ReadStructFromStream<T>(Stream stream)
        {
            var structSize = Marshal.SizeOf(typeof(T));
            var memory = IntPtr.Zero;

            try
            {
                memory = Marshal.AllocCoTaskMem(structSize);
                if (memory == IntPtr.Zero)
                {
                    throw new InvalidOperationException();
                }

                var buffer = new byte[structSize];
                var bytesRead = stream.Read(buffer, 0, structSize);
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