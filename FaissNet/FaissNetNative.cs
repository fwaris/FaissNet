// Copyright (c) .NET Foundation and Contributors.  All Rights Reserved.  See LICENSE in the project root for license information.
#nullable enable
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace FaissNet
{
    public enum MetricType
    {
        METRIC_INNER_PRODUCT = 0, ///< maximum inner product search
		METRIC_L2 = 1,            ///< squared L2 search
		METRIC_L1,                ///< L1 (aka cityblock)
		METRIC_Linf,              ///< infinity distance
		METRIC_Lp,                ///< L_p distance, p is given by a faiss::Index
                                  /// metric_arg

                                  /// some additional metrics defined in scipy.spatial.distance
        METRIC_Canberra = 20,
        METRIC_BrayCurtis,
        METRIC_JensenShannon,
        METRIC_Jaccard, ///< defined as: sum_i(min(a_i, b_i)) / sum_i(max(a_i, b_i))
                        ///< where a_i, b_i > 0
    };

    internal class IndexHandle : SafeHandle
    {
        public IndexHandle(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
        {
        }

        public override bool IsInvalid => this.handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            if (!IsInvalid)
            {
                FaissNetNative.FN_Release(this.handle);
                return true;
            }
            else
            {
                return false;
            }
        }

        internal IntPtr Pointer 
            {
                get{
                if (!IsInvalid) { return this.handle; }
                else { throw new Exception("handle not valid"); }
                }
            }
    }
    internal static partial class FaissNetNative
    {
        [DllImport("FaissNetNative", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError=true)]
        internal static extern IntPtr FN_Create(int dimension, [MarshalAs(UnmanagedType.LPStr)] string description, MetricType metric);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern IntPtr FN_CreateDefault(int dimension, MetricType metric);
        
        [DllImport("FaissNetNative", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError=true)]
        internal static extern void FN_WriteIndex(IntPtr idx, [MarshalAs(UnmanagedType.LPStr)] string path);
        
        [DllImport("FaissNetNative", CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError=true)]
        internal static extern IntPtr FN_ReadIndex([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport("FaissNetNative", SetLastError=true)]
        unsafe internal static extern void FN_Add(IntPtr idx, int n, float* x);

        [DllImport("FaissNetNative", SetLastError=true)]
        unsafe internal static extern void FN_AddWithIds(IntPtr idx, int n, float* x, long * ids);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        unsafe internal static extern void FN_Search(IntPtr idx, int n, float * x, int k, float* distances, long* labels);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        unsafe internal static extern void FN_Assign(IntPtr idx, int n, float* x, long* labels, int k);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        unsafe internal static extern void FN_Train(IntPtr idx, int n, float* x);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern void FN_Reset(IntPtr idx);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern void FN_RemoveIds(IntPtr idx, int n, IntPtr ids);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        unsafe internal static extern void FN_ReconstructBatch(IntPtr idx, int n, long* ids, float* recons);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        unsafe internal static extern void FN_SearchAndReconstruct(IntPtr idx, int n, float* x, int k, float* distances, long* labels, float* recons);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern void FN_MergeFrom(IntPtr idx, IntPtr otherIndex, Int64 add_id);
        
        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern void FN_Release(IntPtr idx);

        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern int FN_Dimension(IntPtr idx);

        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern MetricType FN_MetricType(IntPtr idx);

        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern Int64 FN_Count(IntPtr idx);

        [DllImport("FaissNetNative", SetLastError=true)]
        internal static extern IntPtr FN_GetLastError();
    }
}