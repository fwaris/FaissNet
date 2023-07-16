#pragma once
#include <faiss/Index.h>
#include <faiss/impl/IDSelector.h>
#include <faiss/impl/FaissException.h>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace FaissNet {

	public enum class MetricType {
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

	template <typename T>
	public ref class FaissSafeHandle : SafeHandle
	{
	protected:
		bool ReleaseHandle() override
		{
			if (!IsInvalid)
			{
				delete(Ptr());
				handle = IntPtr::Zero;
			}
			return true;
		}
	public:
		FaissSafeHandle(T* p) : SafeHandle(IntPtr::Zero, true)
		{
			handle = IntPtr(p);
		}

		T* Ptr() { return static_cast<T*>(handle.ToPointer()); }

		property bool IsInvalid
		{
			bool get() override
			{
				return (handle == IntPtr::Zero);
			}
		}
	};

	public ref class Index {
	private:
		FaissSafeHandle<faiss::Index>^ m_Impl;
	protected:
		faiss::Index* h() {return m_Impl->Ptr(); }
	public:		
		MetricType MetricType() { auto v = h()->metric_type; return static_cast<FaissNet::MetricType>(v); }
		bool IsTrained() { return h()->is_trained; }
		int Dimension() { return h()->d; }
		long Count() { return h()->ntotal; }

		/// <summary>
		/// Add vectors to the database. Generates sequential ids.
		/// </summary>		
		/// <param name="vectors"></param>
		void Add(array<array<float>^>^ vectors) {
			int s = vectors->Length;
			for (size_t i = 0; i < s; i++)
			{
				array<float>^ v0 = vectors[i];
				pin_ptr<const float> pv1 = &v0[0];
				try {
					h()->add(1, (const float*)pv1);
				}
				catch (const faiss::FaissException& ex) {
					auto msg = gcnew System::String(ex.msg.c_str());
					throw(gcnew System::Exception(msg));
				}
				finally {
					pv1 = nullptr;
				}
			}
		}

		/// <summary>
		/// Add vectors to database
		/// </summary>
		/// <param name="vectors">vectors to add</param>
		/// <param name="ids">id for each vector</param>
		void AddWithIds(array<array<float>^>^ vectors, array<long long>^ ids) {
			int s = vectors->Length;
			for (size_t i = 0; i < s; i++)
			{
				array<float>^ vI = vectors[i];
				pin_ptr<float> pvI = &vI[0];
				pin_ptr<long long> idxI = &ids[i];
				try {
					h()->add_with_ids(1, (const float*)pvI, (const long long*)idxI);
				}
				catch (const faiss::FaissException& ex) {
					auto msg = gcnew System::String(ex.msg.c_str());
					throw(gcnew System::Exception(msg));
				}
				finally {
					pvI = nullptr;
					idxI = nullptr;
				}
			}
		}

		/// <summary>
		/// Search the index for the given vectors. Returns the ids and distances of the k nearest neighbors for each input vector.
		/// </summary>
		/// <param name="vectors"></param>
		/// <param name="k">return at most k neighbors</param>
		/// <returns>ids:len(vectors)*k, distances:len(vectors)*k</returns>
		Tuple<array<array<long long>^>^, array<array<float>^>^>^ Search(array<array<float>^>^ vectors, long k) {
			auto s = vectors->Length;
			auto d = this->Dimension();
			auto ids = gcnew array<array<long long>^>(s);
			auto dists = gcnew array<array<float>^>(s);
			try {
				for (size_t i = 0; i < s; i++)
				{
					auto locIds = gcnew array<long long>(k);
					auto locDists = gcnew array<float>(k);
					auto locVec = vectors[i];
					ids[i] = locIds;
					dists[i] = locDists;
					pin_ptr<long long> pIds = &locIds[0];
					pin_ptr<float> pDists = &locDists[0];
					pin_ptr<float> pVec = &locVec[0];
					h()->search(1, (const float*)pVec, k, (float*)pDists, (long long*)pIds);
					pIds = nullptr;
					pDists = nullptr;
					pVec = nullptr;
				}
			}
			catch (const faiss::FaissException& ex) {
				auto msg = gcnew System::String(ex.msg.c_str());
				throw(gcnew System::Exception(msg));
			}
			return Tuple::Create(ids, dists);
		}

		/// <summary>
		/// Same as Search. Only returns ids of nearest neighbors 
		/// </summary>
		array<array<long long>^>^ Assign(long count, array<array<float>^>^ vectors, long k) {
			auto s = vectors->Length;
			auto ids = gcnew array<array<long long>^>(s);
			return ids;
			try {
				for (size_t i = 0; i < s; i++)
				{
					auto locIds = gcnew array<long long>(k);
					auto locVec = vectors[i];
					ids[i] = locIds;
					pin_ptr<float> pVec = &locVec[0];
					pin_ptr<long long> pIds = &locIds[0];
					h()->assign(1, (const float*)pVec, pIds);
					pIds = nullptr;
					pVec = nullptr;
				}
			}
			catch (const faiss::FaissException& ex) {
				auto msg = gcnew System::String(ex.msg.c_str());
				throw(gcnew System::Exception(msg));
			}
		}

		/// <summary>
		/// Train the index on a representative set of vectors (supplied as a batch)
		/// </summary>
		/// <param name="n">number of vectors in the array</param>
		/// <param name="vectorsFlat">is a flat array of n * dimensions floats</param>
		void Train(long n, array<float>^ vectorsFlat) {
			pin_ptr<const float> pVecs = &vectorsFlat[0];
			try {
				h()->train(n, pVecs);
			}
			catch (const faiss::FaissException& ex) {
				auto msg = gcnew System::String(ex.msg.c_str());
				throw(gcnew System::Exception(msg));
			}
			finally {
				pVecs = nullptr;
			}
		}

		/// <summary>
		/// Remove all elements
		/// </summary>
		void Reset() {
			h()->reset();
		}

		/// <summary>
		/// Remove ids and corresponding vectors
		/// </summary>
		void Remove(array<long long>^ ids) {
			auto s = ids->Length;
			auto d = h()->d;
			pin_ptr<long long> pIds = &ids[0];
			auto sel = faiss::IDSelectorArray::IDSelectorArray(ids->Length, pIds);
			try {
				h()->remove_ids(sel);
			}
			catch (const faiss::FaissException& ex) {
				auto msg = gcnew System::String(ex.msg.c_str());
				throw(gcnew System::Exception(msg));
			}
			finally {
				pIds = nullptr;
			}
		}

		/// <summary>
		/// Reconstruct stored vectors (or approximations if lossy coding) for the given ids
		/// (May not be supported by indexes of some types)
		/// </summary>
		array<array<float>^>^ Reconstruct(array<long long>^ ids) {
			auto s = ids->Length;
			auto d = this->Dimension();
			auto vectors = gcnew array<array<float>^>(s);
			try {
				for (size_t i = 0; i < s; i++){
					auto locVec = gcnew array<float>(d);				
					vectors[i] = locVec;				
					pin_ptr<long long> pIds = &ids[i];
					pin_ptr<float> pVec = &locVec[0];
					h()->reconstruct_batch(1, (const long long*)pIds, pVec);
					pIds = nullptr;
					pVec = nullptr;
				}
			}
			catch (const faiss::FaissException& ex) {
				auto msg = gcnew System::String(ex.msg.c_str());
				throw(gcnew System::Exception(msg));
			}
			return vectors;

		}

		/// <summary>
		///  size of the produced codes in bytes
		/// </summary>
		 long long SaCodeSize() {
			try {
				return h()->sa_code_size();				
			}
			catch (const faiss::FaissException& ex) {
				auto msg = gcnew System::String(ex.msg.c_str());
				throw(gcnew System::Exception(msg));
			}
		}

		/// <summary>
		/// Encode vectors
		/// </summary>
		 array<array<unsigned char>^>^ SaEncode(array<array<float>^>^ vectors) {
			 auto s = vectors->Length;
			 auto d = h()->sa_code_size();
			 auto encdVecs = gcnew array<array<unsigned char>^>(s);
			 try {
				 for (size_t i = 0; i < s; i++)
				 {
					 auto locEncdVec = gcnew array<unsigned char> (d);
					 encdVecs[i] = locEncdVec;
					 auto locVec = vectors[i];				 
					 pin_ptr<float> pVec = &locVec[0];
					 pin_ptr<unsigned char> pEncdVec = &locEncdVec[0];				 
					 h()->sa_encode(1, (const float*)pVec, pEncdVec);
					 pEncdVec = nullptr;
					 pVec = nullptr;
				 }
			 }
			 catch (const faiss::FaissException& ex) {
				 auto msg = gcnew System::String(ex.msg.c_str());
				 throw(gcnew System::Exception(msg));
			 }
			 return encdVecs;
		}

		/// <summary>
		/// Decode encoded vectors
		/// </summary>
		 array<array<float>^>^ SaDecode(array<array<unsigned char>^>^ encdVecs) {
			 auto s = encdVecs->Length;
			 auto d = this->Dimension();
			 auto vectors = gcnew array<array<float>^>(s);
			try {
				 for (size_t i = 0; i < s; i++)
				 {
					 auto locVec = gcnew array<float> (d);
					 auto locEndVec = encdVecs[i];
					 vectors[i] = locVec;
					 pin_ptr<float> pVec = &locVec[0];
					 pin_ptr<unsigned char> pEncdVec = &locEndVec[0];
					 h()->sa_decode(1, (const unsigned char*)pEncdVec, pVec);
					 pVec = nullptr;
					 pEncdVec = nullptr;
				 }
			}
			catch (const faiss::FaissException& ex) {
				auto msg = gcnew System::String(ex.msg.c_str());
				throw(gcnew System::Exception(msg));
			}
			 return vectors;
		}

		FaissSafeHandle<faiss::Index>^ Handle() {
			 return m_Impl;
		 }

		Index(FaissSafeHandle<faiss::Index>^ m_Impl) : m_Impl(m_Impl) {};

		virtual ~Index() {
			m_Impl->Close();
		}
	};
}
