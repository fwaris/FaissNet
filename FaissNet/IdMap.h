#pragma once
#include "Index.h"
#include <faiss/IndexIDMap.h>
using namespace System;
using namespace System::Runtime::InteropServices;

namespace FaissNet {
	public ref class IdMap {
	private:
		FaissNet::FaissSafeHandle<faiss::IndexIDMap>^ m_Impl;
	protected:
		faiss::IndexIDMap* h() { return m_Impl->Ptr(); }
	public:
		IdMap(FaissNet::FaissSafeHandle<faiss::IndexIDMap>^ m_Impl) : m_Impl(m_Impl) {};
		IdMap(FaissNet::Index^ idx) :
			IdMap(gcnew FaissNet::FaissSafeHandle<faiss::IndexIDMap>(new faiss::IndexIDMap(idx->Handle()->Ptr()))) {};

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
				h()->add_with_ids(1, (const float*)pvI, (const long long*)idxI);
				pvI = nullptr;
				idxI = nullptr;
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
			auto d = h()->d;
			auto ids = gcnew array<array<long long>^>(s);
			auto dists = gcnew array<array<float>^>(s);
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
			return Tuple::Create(ids, dists);
		}

		/// <summary>
		/// Train the index on a representative set of vectors (supplied as a batch)
		/// </summary>
		/// <param name="n">number of vectors in the array</param>
		/// <param name="vectorsFlat">is a flat array of n * dimensions floats</param>
		void Train(long n, array<float>^ vectorsFlat) {
			pin_ptr<const float> pVecs = &vectorsFlat[0];
			h()->train(n, pVecs);
			pVecs = nullptr;
		}

		/// <summary>
		/// Remove all elements
		/// </summary>
		void Reset() {
			h()->reset();
		}

		virtual ~IdMap() {
			m_Impl->Close();
		}
	};

}

