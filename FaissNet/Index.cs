using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FaissNet
{
    using static FaissNetNative;
   
    public class Index : IDisposable
    {
        IndexHandle handle;
        internal Index(IndexHandle h) { handle = h; }

        public static Index Create(int dimension, string constructor, MetricType metric)
        {
            var ptr = FN_Create(dimension, constructor, metric);
            return new Index(new IndexHandle(ptr, true));
        }
        public static Index CreateDefault(int dimension, MetricType metric)
        {
            var ptr = FN_CreateDefault(dimension, metric);
            return new Index(new IndexHandle(ptr, true));
        }

        public void Dispose()
        {
            this.handle.Dispose();
        }

        public string LastError()
        {
            var ptr = FN_GetLastError();
            if (ptr == IntPtr.Zero)
            {
                return "no last error message";
            }
            else
            {
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        public int Dimension { get { return FN_Dimension(this.handle.Pointer); } }
        public Int64 Count { get { return FN_Dimension(this.handle.Pointer); } }
        public MetricType MetricType { get { return FN_MetricType (this.handle.Pointer); } }

        public void Add(float[][] vectors)
        {
            try
            {
                foreach (var vector in vectors)
                {
                    var sp = new ReadOnlySpan<float>(vector);
                    unsafe
                    {
                        fixed(float * ptr = sp)
                        {
                            FN_Add(this.handle.Pointer, 1, ptr);
                        }
                    }
                }
            }
            catch (SEHException ex) {
                // Get the underlying message from the SEHException.
                var msg = this.LastError();
                throw new Exception(msg, ex);
            }
        }
        public void AddWithIds(float[][] vectors, long[] ids)
        {
            var idSpan = new ReadOnlySpan<long>(ids);
            for (int i = 0; i < vectors.Length; i++)
            {
                //its seems, by default, faiss copies the data into its own memory
                var sp = new ReadOnlySpan<float>(vectors[i]);
                unsafe {
                    fixed (float* ptrVec = sp) {
                        fixed (long* ptrIdx = idSpan.Slice(i,1))
                            FN_AddWithIds(this.handle.Pointer, 1, ptrVec, ptrIdx);
                    }
                }
            }           
        }
    }
}
