using System;
using System.Collections;					// ArrayList
using System.Diagnostics;					// Process
using System.Runtime.InteropServices;		// DllImport
using System.Text;							// StringBuilder

namespace TrayIconBuster {
	/// <summary>
	/// LP_Process allows to send Windows messages and exchange data with
	/// windows in another process; this may require pointers that are valid
	/// in the other process.
	/// </summary>
	public class LP_Process : IDisposable {
		//================================================================================
		// class constants
		//================================================================================
		private const uint PROCESS_VM_OPERATION=		0x0008;
		private const uint PROCESS_VM_READ=				0x0010;
		private const uint PROCESS_VM_WRITE=			0x0020;
		private const uint PROCESS_QUERY_INFORMATION=	0x0400;

		private const uint MEM_COMMIT=					0x1000;
		private const uint MEM_RELEASE=					0x8000;
		private const uint PAGE_READWRITE=				0x0004;

		//================================================================================
		// object data
		//================================================================================
		/// <summary>process handle</summary>
		private IntPtr hProcess;

		/// <summary>process ID</summary>
		private uint ownerProcessID;

		/// <summary>list of allocations in this process</summary>
		private ArrayList allocations=new ArrayList();
		//================================================================================
		// structors
		//================================================================================
		/// <summary>
		/// Creates an instance of LP_Process, owner of the window.
		/// </summary>
		/// <param name="hWnd"></param>
		public LP_Process(IntPtr hWnd) {
			GetWindowThreadProcessId(hWnd, ref ownerProcessID);
			log("owner: procID="+ownerProcessID+"=0x"+ownerProcessID.ToString("X4"));
			hProcess=OpenProcess(PROCESS_VM_OPERATION|PROCESS_VM_READ|
				PROCESS_VM_WRITE|PROCESS_QUERY_INFORMATION, false, ownerProcessID);
			log("handle to owner process="+hProcess.ToString("X8"));
		}

		/// <summary>
		/// Disposes of an LP_Process (closing all open handles).
		/// </summary>
		public void Dispose() {
			if (hProcess!=IntPtr.Zero) {
				foreach (IntPtr ptr in allocations) {
					VirtualFreeEx(hProcess, ptr, 0, MEM_RELEASE);
				}
				CloseHandle(hProcess);
			}
		}

		private void log(string s) {
			//Console.WriteLine(s);
		}

		/// <summary>
		/// Gets the file name of the process image.
		/// </summary>
		/// <returns></returns>
		public string GetImageFileName() {
			StringBuilder sb=new StringBuilder(1024);
			bool OK=GetProcessImageFileName(hProcess, sb, sb.Capacity-1);
			if(!OK) return null;
			return sb.ToString();
		}

		//================================================================================
		// memory operations
		//================================================================================
		/// <summary>
		/// Allocates a chunck of memory in the process.
		/// The memory gets freed when the LP_Process object is disposed.
		/// </summary>
		/// <param name="managedObject"></param>
		/// <returns></returns>
		public IntPtr Allocate(object managedObject) {
			int size=Marshal.SizeOf(managedObject);
			IntPtr ptr=VirtualAllocEx(hProcess, 0, size, MEM_COMMIT, PAGE_READWRITE);
			log("ptr="+ptr.ToString("X8"));
			if(ptr!=IntPtr.Zero) allocations.Add(ptr);
			return ptr;
		}

		/// <summary>
		/// Reads an object's data from the process memory at ptr.
		/// </summary>
		/// <param name="obj">Must be a reference type (no struct!)</param>
		/// <param name="ptr"></param>
		public void Read(object obj, IntPtr ptr) {
			using(LP_Pinner pin=new LP_Pinner(obj)) {
				uint bytesRead=0;
				int size=Marshal.SizeOf(obj);
				if (!ReadProcessMemory(hProcess, ptr, pin.Ptr, size, ref bytesRead)) {
					int err=GetLastError();
					string s="Read failed; err="+err+"; bytesRead="+bytesRead;
					throw new ApplicationException(s);
				}
			}
		}

		/// <summary>
		/// Reads a string from the process memory at ptr.
		/// </summary>
		/// <param name="size"></param>
		/// <param name="ptr"></param>
		public string ReadString(int size, IntPtr ptr) {
			StringBuilder sb=new StringBuilder(size);
			uint bytesRead=0;
			if(!ReadProcessMemory(hProcess, ptr, sb, size, ref bytesRead)) {
				int err=GetLastError();
				string s="Read failed; err="+err+"; bytesRead="+bytesRead;
				throw new ApplicationException(s);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Write an object's data to the process memory at ptr.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="ptr"></param>
		/// <param name="size"></param>
		public void Write(object obj, int size, IntPtr ptr) {
			using (LP_Pinner pin=new LP_Pinner(obj)) {
				uint bytesWritten=0;
				if(!WriteProcessMemory(hProcess, ptr, pin.Ptr, size, ref bytesWritten)) {
					int err=GetLastError();
					string s="Write failed; err="+err+"; bytesWritten="+bytesWritten;
					throw new ApplicationException(s);
				}
			}
		}

		//================================================================================
		// static methods (getting the working set)
		//================================================================================
		/// Retrieves the identifier of the thread that created the specified window
		/// and the identifier of the process that created the window. 
		[DllImport("user32.dll", CallingConvention=CallingConvention.StdCall)]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint procId);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)] 
		private static extern IntPtr OpenProcess(uint access, bool inheritHandle,
			uint procID);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)] 
		private static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)] 
		private static extern IntPtr VirtualAllocEx(IntPtr hProcess, int address,
			int size, uint allocationType, uint protection);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)] 
		private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr address,
			int size, uint freeType);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)] 
		private static extern bool WriteProcessMemory(IntPtr hProcess,
			IntPtr otherAddress, IntPtr localAddress, int size,
			ref uint bytesWritten);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)] 
		private static extern bool ReadProcessMemory(IntPtr hProcess,
			IntPtr otherAddress, IntPtr localAddress, int size,
			ref uint bytesRead);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)]
		private static extern bool ReadProcessMemory(IntPtr hProcess,
			IntPtr otherAddress, StringBuilder localAddress, int size,
			ref uint bytesRead);

		[DllImport("psapi.dll", CallingConvention=CallingConvention.StdCall)] 
		private static extern bool GetProcessImageFileName(IntPtr hProcess,
			StringBuilder fileName, int fileNameSize);

		[DllImport("kernel32.dll", CallingConvention=CallingConvention.StdCall)]
		public static extern int GetLastError();
	}
}

