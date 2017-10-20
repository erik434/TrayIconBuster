using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;		// DllImport
using System.Text;
using System.Windows.Forms;					// SystemInformation

namespace TrayIconBuster {
	class TrayIconBuster {
		private const uint TB_BUTTONCOUNT=			0x0418;	// WM_USER+24
		private const uint TB_GETBUTTON=			0x0417;	// WM_USER+23
		private const uint TB_DELETEBUTTON=			0x0416;	// WM_USER+22

		private static object key=new object(); // concurrency protection

		// for debug purposes only
		private static void log(string s) {
			Console.WriteLine(DateTime.Now.ToLongTimeString()+" "+s);
		}

		public static bool Is64BitWindows() {
			bool is64bitWindows=false;
			try {
				SYSTEM_INFO si;
				const int PROCESSOR_ARCHITECTURE_AMD64=9;	// x64 (AMD or Intel)
				GetSystemInfo(out si);
				if(si.processorArchitecture==PROCESSOR_ARCHITECTURE_AMD64) is64bitWindows=true;
			} catch {}
			log("is64bitWindows="+is64bitWindows);
			return is64bitWindows;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SYSTEM_INFO {
			public ushort processorArchitecture;
			ushort reserved;
			public uint pageSize;
			public IntPtr minimumApplicationAddress;
			public IntPtr maximumApplicationAddress;
			public UIntPtr activeProcessorMask;
			public uint numberOfProcessors;
			public uint processorType;
			public uint allocationGranularity;
			public ushort processorLevel;
			public ushort processorRevision;
		}

		[DllImport("kernel32.dll")]
		static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

		/// <summary>
		/// The actual trayIconBuster
		/// </summary>
		/// <returns>The number of tray icons removed.</returns>
		public unsafe static uint RemovePhantomIcons() {
			bool is64bitWindows=Is64BitWindows();
			ToolBarButton64 tbb64=new ToolBarButton64();
			ToolBarButton32 tbb32=new ToolBarButton32();
			TrayData td=new TrayData();
			bool foundSomeExe=false;
			uint totalRemovedCount=0;
			uint totalItemCount=0;
			// for safety reasons we perform two passes:
			// pass1 = search for my own NotifyIcon
			// pass2 = search phantom icons and remove them
			//         pass2 doesnt happen if pass1 fails
			lock(key) {			// prevent concurrency problems
				for(int pass=1; pass<=2; pass++) {
					for(int kind=0; kind<2; kind++) {
						IntPtr hWnd=IntPtr.Zero;
						if(kind==0) {
							// get the regular icon collection that exists on all Windows versions
							FindNestedWindow(ref hWnd, "Shell_TrayWnd");
							FindNestedWindow(ref hWnd, "TrayNotifyWnd");
							FindNestedWindow(ref hWnd, "SysPager");
							FindNestedWindow(ref hWnd, "ToolbarWindow32");
						} else {
							// get the hidden icon collection that exists since Windows 7
							try {
								FindNestedWindow(ref hWnd, "NotifyIconOverflowWindow");
								FindNestedWindow(ref hWnd, "ToolbarWindow32");
							} catch {
								// fail silently, as NotifyIconOverflowWindow did not exist prior to Win7
								break;
							}
						}
						// create an object so we can exchange data with other process
						using(LP_Process process=new LP_Process(hWnd)) {
							IntPtr remoteButtonPtr;
							if(is64bitWindows) {
								remoteButtonPtr=process.Allocate(tbb64);
							} else {
								remoteButtonPtr=process.Allocate(tbb32);
							}
							process.Allocate(td);
							uint itemCount=(uint)SendMessage(hWnd, TB_BUTTONCOUNT,
								IntPtr.Zero, IntPtr.Zero);
							//log("There are "+itemCount+" tray icons (some of them hidden)");
							uint removedCount=0;
							for(uint item=0; item<itemCount; item++) {
								totalItemCount++;
								// index changes when previous items got removed !
								uint item2=item-removedCount;
								uint SOK=(uint)SendMessage(hWnd, TB_GETBUTTON,
									new IntPtr(item2), remoteButtonPtr);
								if(SOK!=1) throw new ApplicationException("TB_GETBUTTON failed");
								if(is64bitWindows) {
									process.Read(tbb64, remoteButtonPtr);
									process.Read(td, tbb64.dwData);
								} else {
									process.Read(tbb32, remoteButtonPtr);
									process.Read(td, tbb32.dwData);
								}
								IntPtr hWnd2=td.hWnd;
								if(hWnd2==IntPtr.Zero) throw new ApplicationException("Invalid window handle");
								using(LP_Process proc=new LP_Process(hWnd2)) {
									string filename=proc.GetImageFileName();
									if(pass==1&&filename!=null) {
										filename=filename.ToLower();
										if(filename.EndsWith(".exe")) {
											foundSomeExe=true;
											log("(kind="+kind+") found real icon created by: "+filename);
											break;
										}
									}
									// a phantom icon has no imagefilename
									if(pass==2&&filename==null) {
										SOK=(uint)SendMessage(hWnd, TB_DELETEBUTTON,
											new IntPtr(item2), IntPtr.Zero);
										if(SOK!=1) throw new ApplicationException("TB_DELETEBUTTON failed");
										removedCount++;
										totalRemovedCount++;
									}
								}
							}
						}
					} // next kind
					// if I did not see myself, I will not run the second
					// pass, which would try and remove phantom icons
					if(totalItemCount!=0&&!foundSomeExe) throw new ApplicationException(
				   "Failed to find any real icon");
				}
			} // release lock
			log(totalItemCount.ToString()+" icons found, "+totalRemovedCount+" icons removed");
			return totalRemovedCount;
		}

		// Find a topmost or nested window with specified name
		private static void FindNestedWindow(ref IntPtr hWnd, string name) {
			if(hWnd==IntPtr.Zero) {
				hWnd=FindWindow(name, null);
			} else {
				hWnd=FindWindowEx(hWnd, IntPtr.Zero, name, null);
			}
			if(hWnd==IntPtr.Zero) throw new ApplicationException("Failed to locate window "+name);
		}

		[DllImport("user32.dll", EntryPoint="SendMessageA",
			CallingConvention=CallingConvention.StdCall)]
		public static extern IntPtr SendMessage(IntPtr Hdc, uint Msg_Const,
			IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", EntryPoint="FindWindowA",
			 CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);

		[DllImport("user32.dll", EntryPoint="FindWindowExA",
			 CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Ansi)]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter,
			string lpszClass, string lpszWindow);

		/// <summary>
		/// ToolBarButton struct used for TB_GETBUTTON message.
		/// </summary>
		/// <remarks>We use a class so LP_Process.Read can fill this. The struct padding varies
		/// depending on Win32/Win64.
		/// ref: http://msdn.microsoft.com/en-us/library/windows/desktop/bb760476(v=vs.85).aspx</remarks>
		[StructLayout(LayoutKind.Sequential)]
		public class ToolBarButton32 {
			public uint iBitmap;		//  0
			public uint idCommand;		//  4
			public byte fsState;		//  8
			public byte fsStyle;		//  9
			private byte bReserved0;	// 10: 2 padding bytes added so IntPtr is at multiple of 4
			private byte bReserved1;	// 11
			public IntPtr dwData;		// 12: points to tray data
			public uint iString;		// 16
		}
		[StructLayout(LayoutKind.Sequential)]
		public class ToolBarButton64 {
			public uint iBitmap;		//  0
			public uint idCommand;		//  4
			public byte fsState;		//  8
			public byte fsStyle;		//  9
			private byte bReserved0;	// 10: 6 padding bytes added so IntPtr is at multiple of 8
			private byte bReserved1;	// 11
			private byte bReserved2;	// 12
			private byte bReserved3;	// 13
			private byte bReserved4;	// 14
			private byte bReserved5;	// 15
			public IntPtr dwData;		// 16: points to tray data
			public uint iString;		// 24
		}

		/// <summary>
		/// TrayData struct used for extra info for ToolBarButton.
		/// </summary>
		/// <remarks>We use a class so LP_Process.Read can fill this.
		/// No padding required, layout is always fine (and irrelevant as we only use the first field!)</remarks>
		[StructLayout(LayoutKind.Sequential)]
		public class TrayData {
			public IntPtr hWnd;				//  0
			public uint uID;				//  4 or  8
			public uint uCallbackMessage;	//  8 or 12
			private uint reserved0;			// 12 or 16
			private uint reserved1;			// 16 or 20
			public IntPtr hIcon;			// 20 or 24
		}
	}
}