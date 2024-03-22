using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static MPLLib.MemoryHandle.Kernal32;

namespace MPLLib.MemoryHandle
{


    public static class Kernal32Handle
    {
        public static IntPtr OpenProcessHandle(Process process)
        {
            return OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, process.Id);
        }
        public static IEnumerable<byte[]> ReadProcessMemoryBuffer(IntPtr processHandle)
        {
            SYSTEM_INFO sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);
            IntPtr proc_min_address = sys_info.minimumApplicationAddress;
            IntPtr proc_max_address = sys_info.maximumApplicationAddress;
            long proc_min_address_l = (long)proc_min_address;
            long proc_max_address_l = (long)proc_max_address;
            MEMORY_BASIC_INFORMATION64 mem_basic_info = new MEMORY_BASIC_INFORMATION64();

            int bytesRead = 0;

            while (proc_min_address_l < proc_max_address_l)
            {
                int code = VirtualQueryEx(processHandle, proc_min_address, out mem_basic_info, 48u);
                if (code == 0)
                {
                    yield return null;
                    //Console.WriteLine($"ERROR : 0x{code} : {GetErrorMessage(code)}");
                }
                else
                {
                    if (mem_basic_info.Protect ==
                    PAGE_READWRITE && mem_basic_info.State == MEM_COMMIT)
                    {
                        byte[] buffer = new byte[mem_basic_info.RegionSize];

                        // read everything in the buffer above
                        ReadProcessMemory((int)processHandle, mem_basic_info.BaseAddress, buffer, mem_basic_info.RegionSize, ref bytesRead);

                        yield return buffer;
                    }
                    // move to the next memory chunk
                    proc_min_address_l += mem_basic_info.RegionSize;
                    proc_min_address = new IntPtr(proc_min_address_l);
                    //Console.WriteLine($"Read Size : {mem_basic_info.RegionSize}");
                }
            }
            yield break;
        }


        public static void Do()
        {

            SYSTEM_INFO sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);

            IntPtr proc_min_address = sys_info.minimumApplicationAddress;
            IntPtr proc_max_address = sys_info.maximumApplicationAddress;

            long proc_min_address_l = (long)proc_min_address;
            long proc_max_address_l = (long)proc_max_address;

            Process[] processes = Process.GetProcessesByName("ridibooks");
            Process process = processes[0];

            IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, process.Id);
            //IntPtr processHandle = process.Handle;

            StreamWriter sw = new StreamWriter(@"C:/Users/white/OneDrive/문서/dump.dat");
            

            MEMORY_BASIC_INFORMATION64 mem_basic_info = new MEMORY_BASIC_INFORMATION64();

            int bytesRead = 0;

            while (proc_min_address_l < proc_max_address_l)
            {
                int code = VirtualQueryEx(processHandle, proc_min_address, out mem_basic_info, 48u);
                if(code==0)
                {
                    int err = GetLastError();
                    Console.WriteLine($"ERROR : 0x{err} : {GetErrorMessage(err)}");
                }

                
                
                // if this memory chunk is accessible
                if (mem_basic_info.Protect ==
                PAGE_READWRITE && mem_basic_info.State == MEM_COMMIT)
                {
                    byte[] buffer = new byte[mem_basic_info.RegionSize];

                    // read everything in the buffer above
                    ReadProcessMemory((int)processHandle, mem_basic_info.BaseAddress, buffer, mem_basic_info.RegionSize, ref bytesRead);

                    for (long i = 0; i < mem_basic_info.RegionSize; i++)
                        sw.Write((char)buffer[i]);

                    /*
                    // then output this in the file
                    for (long i = 0; i < mem_basic_info.RegionSize; i++)
                        sw.WriteLine("0x{0} : {1}",
                        (mem_basic_info.BaseAddress + i).ToString("X"), (char)buffer[i]);
                    */
                }
                // move to the next memory chunk
                proc_min_address_l += mem_basic_info.RegionSize;
                proc_min_address = new IntPtr(proc_min_address_l);

                Console.WriteLine(proc_min_address_l); 
            }

            sw.Close();
        }

        public static String GetErrorMessage(Int32 errcode)
        {
            String errmsg;
            FormatMessage(0x1300, IntPtr.Zero, errcode, 0x400, out errmsg, 260, IntPtr.Zero);
            return errmsg;
        }
    }

    public static class Kernal32
    {
        [DllImport("kernel32")]
        public static extern Int32 GetLastError();
        [DllImport("kernel32", CharSet = CharSet.Auto)]
        public static extern Int32 FormatMessage(Int32 dwFlags, IntPtr lpSource, Int32 dwMessageId, Int32 dwLanguageId, out String lpBuffer, Int32 dwSize, IntPtr lpArguments);


        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);


        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, long lpBaseAddress, byte[] lpBuffer, long dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(int hProcess, long lpBaseAddress, byte[] lpBuffer, long dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int MEM_COMMIT = 0x00001000;
        public const int PAGE_READWRITE = 0x04;
        public const int PROCESS_WM_READ = 0x0010;
        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;  // minimum address
            public IntPtr maximumApplicationAddress;  // maximum address
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }
        [StructLayout(LayoutKind.Sequential)]

        public struct MEMORY_BASIC_INFORMATION32
        {
            public int BaseAddress;
            public int AllocationBase;
            public int AllocationProtect;
            public int RegionSize;
            public int State;
            public int Protect;
            public int lType;
        }
        public struct MEMORY_BASIC_INFORMATION64
        {
            public Int64 BaseAddress;
            public Int64 AllocationBase;
            public Int32 AllocationProtect;
            public Int32 __alignment1;
            public Int64 RegionSize;
            public int State;
            public int Protect;
            public int Type;
            public int __alignment2;
        }
        public struct MEMORY_BASIC_INFORMATION_DEBUG
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public Int32 AllocationProtect;
            public IntPtr RegionSize;
            public Int16 PartitionId;
            public Int32 State;
            public Int32 Protect;
            public Int32 Type;
        }

    }
    

}
