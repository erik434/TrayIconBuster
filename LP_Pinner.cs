using System;
using System.Runtime.InteropServices;

namespace TrayIconBuster
{
    /// <summary>
    /// An LP_Pinner object pins an object for as long as the LP_Pinner object is alive.
    /// </summary>
    /// <remarks>
    /// This is the preferred use when the pointer value is not needed:
    ///		using (new LP_Pinner(objectThatNeedsPinning)) {
    ///			.. use the object here, it is pinned now ...
    ///		}
    /// This is the preferred use when the pointer value is needed:
    ///		using (LP_Pinner pinning=new LP_Pinner(objectThatNeedsPinning)) {
    ///		    IntPtr ptr=pinning.Ptr;
    ///			.. use the object here, it is pinned now ...
    ///		}
    /// </remarks>
    public class LP_Pinner : IDisposable
    {
        private GCHandle handle;
        private bool disposed;
        private IntPtr ptr;

        /// <summary>
        /// Creates an instance op LP_Pinner, and pins the argument.
        /// </summary>
        /// <param name="obj"></param>
        public LP_Pinner(object obj)
        {
            //env.log(0,"AllocPinned "+obj.GetType().Name);
            handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            ptr = handle.AddrOfPinnedObject();
        }

        /// <summary>
        /// Undoes the pinning.
        /// </summary>
        ~LP_Pinner()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes of the object's internal resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                //env.log(0,"Free");
                disposed = true;
                handle.Free();
                ptr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Returns the pointer to the pinned object.
        /// </summary>
        public IntPtr Ptr { get { return ptr; } }
    }
}
