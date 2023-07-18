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

        #region factory
        /// <summary>
        /// Create an index of the type specified by the constructor parameter
        /// See <see href="https://github.com/facebookresearch/faiss/wiki/The-index-factory">https://github.com/facebookresearch/faiss/wiki/The-index-factoryfaiss</see> for syntax.
        /// </summary>
        /// <param name="dimension">vector dimension</param>
        /// <param name="constructor">faiss index constructor string</param>
        /// <param name="metric">distance metric</param>
        /// <returns></returns>
        public static Index Create(int dimension, string constructor, MetricType metric)
        {
            var ptr = FN_Create(dimension, constructor, metric);
            return new Index(new IndexHandle(ptr, true));
        }
        /// <summary>
        /// Create the default index type using the string "IDMap2,HNSW32"
        /// </summary>
        /// <param name="dimension">vector dimension</param>
        /// <param name="metric">distance metric</param>
        /// <returns></returns>
        public static Index CreateDefault(int dimension, MetricType metric)
        {
            var ptr = FN_CreateDefault(dimension, metric);
            return new Index(new IndexHandle(ptr, true));
        }
        #endregion

        #region IO
        public static Index Load(string path)
        {
            return Run<Index>(() =>
            {
                var handle = FN_ReadIndex(path);
                return new Index(new IndexHandle(handle, true));
            });
        }

        public void Save(string path)
        {
            Do(() => FN_WriteIndex(this.handle.Pointer, path));
        }

        #endregion

        public void Dispose()
        {
            this.handle.Dispose();
        }

        #region error handling

        /// <summary>
        /// Get the last error set by 
        /// </summary>
        public static string LastError()
        {
            var ptr = FN_GetLastError();
            if (ptr == IntPtr.Zero)
            {
                return "no last error message";
            }
            else
            {
                try { return Marshal.PtrToStringAnsi(ptr); } catch { return "unkown error"; }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Run<T>(Func<T> comp)
        {
            try
            {
                return comp();
            }
            catch (SEHException ex)
            {
                var msg = LastError();
                throw new Exception(msg, ex);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Do(Action comp)
        {
            try
            {
                comp();
            }
            catch (SEHException ex)
            {
                var msg = LastError();
                throw new Exception(msg, ex);
            }
        }

        #endregion

        public Int64 Count { get { return Run<long>(() => FN_Count(this.handle.Pointer)); } }
        public MetricType MetricType { get { return Run<MetricType>(() => FN_MetricType(this.handle.Pointer)); } }
        public int Dimension { get { return Run<int>(() => FN_Dimension(this.handle.Pointer)); } }

        /// <summary>
        /// Add vectors to index. May not be defined for some index types.
        /// </summary>
        /// <param name="n">the number of vectors, each of the specified dimension</param>
        /// <param name="vectors">vector data of size n * dimension</param>
        public void AddFlat(int n, float[] vectors)
        {
            Do(() =>
            {
                var sp = new ReadOnlySpan<float>(vectors);
                unsafe
                {
                    fixed (float* ptr = sp)
                    {
                        FN_Add(this.handle.Pointer, n, ptr);
                    }
                }
            });
        }

        /// <summary>
        /// Similar to AddFlat but for jagged arrays,
        /// May not be defined for some index types.
        /// </summary>
        /// <param name="vectors">jagged array of vectors. Each vector is of size dimension</param>
        public void Add(float[][] vectors)
        {
            Do(() =>
            {
                foreach (var vector in vectors)
                {
                    var sp = new ReadOnlySpan<float>(vector);
                    unsafe
                    {
                        fixed (float* ptr = sp)
                        {
                            FN_Add(this.handle.Pointer, 1, ptr);
                        }
                    }
                };
            });
        }

        /// <summary>
        /// Add vectors and ids to index. 
        /// May not be defined for some index types.
        /// </summary>
        /// <param name="n">number of vectors, each of the specified dimension</param>
        /// <param name="vectors">vector data of size n * dimension</param>
        /// <param name="ids">n-length array of ids</param>
        public void AddWithIdsFlat(int n, float[] vectors, long[] ids)
        {
            Do(() =>
            {
                var idSpan = new ReadOnlySpan<long>(ids);
                var sp = new ReadOnlySpan<float>(vectors);
                unsafe
                {
                    fixed (float* ptrVec = sp)
                    fixed (long* pIds = idSpan)
                    {
                        FN_AddWithIds(this.handle.Pointer, n, ptrVec, pIds);
                    }
                }
            });
        }

        /// <summary>
        /// Similar to AddWithIdsFlat but for jagged arrays.
        /// May not be defined for some index types.
        /// </summary>
        /// <param name="vectors">jagged array of vectors, each of the specified dimension</param>
        /// <param name="ids">array of ids of the same length as vectors</param>
        public void AddWithIds(float[][] vectors, long[] ids)
        {
            Do(() =>
            {
                var idSpan = new ReadOnlySpan<long>(ids);
                for (int i = 0; i < vectors.Length; i++)
                {
                    //its seems, by default, faiss copies the data into its own memory
                    var sp = new ReadOnlySpan<float>(vectors[i]);
                    unsafe
                    {
                        fixed (float* ptrVec = sp)
                        {
                            fixed (long* ptrIdx = idSpan.Slice(i, 1))
                                FN_AddWithIds(this.handle.Pointer, 1, ptrVec, ptrIdx);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Search for similar neighbors in the index for the given vectors
        /// </summary>
        /// <param name="n">number of input vectors</param>
        /// <param name="vectors">flat array of input vectors of size n * dimension </param>
        /// <param name="k">number of neighbors to return</param>
        /// <returns>tuple:(distances [n*k], neighbor ids [n*k])</returns>
        public Tuple<float[], long[]> SearchFlat(int n, float[] vectors, int k)
        {
            var distances = new float[n * k];
            var labels = new long[n * k];
            Do(() =>
            {
                var distSpan = new Span<float>(distances);
                var lblSpan = new Span<long>(labels);
                var vecSpan = new ReadOnlySpan<float>(vectors);
                unsafe
                {
                    fixed (float* ptrDists = distSpan)
                    fixed (float* ptrVecs = vecSpan)
                    fixed (long* ptrLbls = lblSpan)
                    {
                        FN_Search(this.handle.Pointer, n, ptrVecs, k, ptrDists, ptrLbls);
                    }
                }
            });
            return Tuple.Create(distances, labels);
        }

        /// <summary>
        /// Similar to SearchFlat but for jagged arrays
        /// </summary>
        /// <param name="vectors">n * dimension jagged array of floats</param>
        /// <param name="k">number of neighbors to return</param>
        /// <returns>tuple:(distances [n][k], neighbor ids [n][k])</returns>
        public Tuple<float[][], long[][]> Search(float[][] vectors, int k)
        {
            var vecs = vectors.SelectMany(r => r).ToArray();
            var (dists, lbls) = this.SearchFlat(vectors.Length, vecs, k);
            return Tuple.Create(dists.Chunk(k).ToArray(), lbls.Chunk(k).ToArray());
        }

        /// <summary>
        /// Search for the neighbors of the given vectors and also return the neighbors' vectors.
        /// May not be defined for some index types.
        /// </summary>
        /// <param name="n">number of input vectors</param>
        /// <param name="vectors">flat array of input vectors of size n * dimension</param>
        /// <param name="k">number of neighbors</param>
        /// <returns>tuple:(distances [n*k], neighbor ids [n*k], neighbor vectors [n*k*dimension])</returns>
        public Tuple<float[], long[], float[]> SearchAndReconstruct(int n, float[] vectors, int k)
        {
            var distances = new float[n * k];
            var labels = new long[n * k];
            var recons = new float[n * k * this.Dimension];
            Do(() =>
            {
                var distSpan = new Span<float>(distances);
                var lblSpan = new Span<long>(labels);
                var reconSpan = new Span<float>(recons);
                var vecSpan = new ReadOnlySpan<float>(vectors);
                unsafe
                {
                    fixed (float* ptrDists = distSpan)
                    fixed (float* ptrVecs = vecSpan)
                    fixed (long* ptrLbls = lblSpan)
                    fixed (float* ptrRecons = reconSpan)
                    {
                        FN_SearchAndReconstruct(this.handle.Pointer, n, ptrVecs, k, ptrDists, ptrLbls, ptrRecons);
                    }
                }
            });
            return Tuple.Create(distances, labels, recons);
        }
        /// <summary>
        /// Get the vectors associated with the given ids.
        /// </summary>
        /// <param name="ids">array of ids</param>
        /// <returns>vectors [ids.Length * dimension]</returns>
        public float[] Reconstruct(long[] ids)
        {
            var recons = new float[ids.Length * this.Dimension];
            Do(() =>
            {
                var idSpan = new ReadOnlySpan<long>(ids);
                var reconSpan = new Span<float>(recons);
                unsafe
                {
                    fixed (long* ptrIds = idSpan)
                    fixed (float* ptrVecs = reconSpan)
                    {
                        FN_ReconstructBatch(this.handle.Pointer, ids.Length, ptrIds, ptrVecs);
                    }
                }
            });
            return recons;
        }

        /// <summary>
        /// Train the index give an representative set of vectors.
        ///  May not be defined for some index types.
        /// </summary>
        /// <param name="n">number of training vectors</param>
        /// <param name="vectors">flat array of vectors [n * dimension]</param>
        /// 
        public void Train(int n, float[] vectors)
        {
            Do(() =>
            {
                var vecSpan = new ReadOnlySpan<float>(vectors);
                unsafe
                {
                    fixed (float* ptrVecs = vecSpan)
                    {
                        FN_Train(this.handle.Pointer, n,ptrVecs);
                    }
                }
            });
        }
        
        /// <summary>
        /// Find the ids for the given vectors
        /// May not be defined for some index types.
        /// </summary>
        /// <param name="n">number of training vectors</param>
        /// <param name="vectors">flat array of vectors [n * dimension]</param>
        /// <returns>vector of size n containing the associated ids</returns>
        public long[] Assign(int n, float[] vectors)
        {   
            var ids = new long[n];
            Do(() =>
            {
                var vecSpan = new ReadOnlySpan<float>(vectors);
                var idsSpan = new Span<long>(ids);
                unsafe
                {
                    fixed (float* ptrVecs = vecSpan)
                    fixed (long* ptrIds = idsSpan)
                    {
                        FN_Assign(this.handle.Pointer, n, ptrVecs, ptrIds, 1);
                    }
                }
            });
            return ids;
        }        

        public void RemoveIds(long[] ids) 
        {
            Do(() =>
            {
                var idsSpan = new ReadOnlySpan<long>(ids);
                unsafe
                {
                    fixed(long* ptrIds = idsSpan)
                    {
                        FN_RemoveIds(this.handle.Pointer, ids.Length, ptrIds);
                    }
                }
            });
        }
    }
}
