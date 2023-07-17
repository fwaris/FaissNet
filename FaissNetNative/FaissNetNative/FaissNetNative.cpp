﻿// FaissNetNative.cpp : Defines the entry point for the application.
//

#include "FaissNetNative.h"
#include <faiss/impl/IDSelector.h>

using namespace std;


EXPORT_API(void*) FN_Create(int dimension, const char* description, faiss::MetricType metric)  noexcept(false) {
    FN_CATCH(
        faiss::Index* index = faiss::index_factory(dimension, description, metric);
        return index;
    )
}

EXPORT_API(void*) FN_CreateDefault(int dimension, faiss::MetricType metric) noexcept(false)
{
    FN_CATCH(return FN_Create(dimension, "IDMap2,HNSW32", metric);)
}

EXPORT_API(void) FN_WriteIndex(faiss::Index* idx, const char* path) noexcept(false)
{
    FN_CATCH(faiss::write_index(idx, path);)
}

EXPORT_API(void*) FN_ReadIndex(const char* path) noexcept(false)
{
    FN_CATCH(return faiss::read_index(path);)
}

EXPORT_API(void) FN_Add(faiss::Index* idx, int n, const float* x) noexcept(false)
{
    FN_CATCH(idx->add(n, x);)
}

EXPORT_API(void) FN_AddWithIds(faiss::Index* idx, int n, const float* x, const long long* ids) noexcept(false)
{
    FN_CATCH(idx->add_with_ids(n, x, ids);)
}

EXPORT_API(void) FN_Search(faiss::Index* idx, int n, const float* x, int k, float* distances, long long* labels) noexcept(false)
{
    FN_CATCH(idx->search(n, x, k, distances, labels);)
}

EXPORT_API(void) FN_Assign(faiss::Index* idx, int n, const float* x, long long* labels, int k) noexcept(false)
{
    FN_CATCH(idx->assign(n, x, labels, k);)
}

EXPORT_API(void) FN_Train(faiss::Index* idx, int n, const float* x) noexcept(false)
{
    FN_CATCH(idx->train(n, x);)
}

EXPORT_API(void) FN_Reset(faiss::Index* idx) noexcept(false)
{
    FN_CATCH(idx->reset();)
}

EXPORT_API(void) FN_RemoveIds(faiss::Index* idx, int n, long long* ids) noexcept(false)
{
   FN_CATCH( 
       auto sel = faiss::IDSelectorArray::IDSelectorArray(n, ids);
        idx->remove_ids(sel);
   )
}

EXPORT_API(void) FN_ReconstructBatch(faiss::Index* idx, int n, const long long* ids, float* recons) noexcept(false)
{
   FN_CATCH(idx->reconstruct_batch(n, ids, recons);)
}

EXPORT_API(void) FN_SearchAndReconstruct(faiss::Index* idx, int n, const float* x, int k, float* distances, long long* labels, float* recons) noexcept(false)
{
    FN_CATCH(idx->search_and_reconstruct(n, x, k, distances, labels, recons);)
}

EXPORT_API(void) FN_MergeFrom(faiss::Index* idx, faiss::Index* otherIndex, long long add_id) noexcept(false)
{
   FN_CATCH(idx->merge_from(*otherIndex, add_id);)
}

EXPORT_API(void) FN_Release(faiss::Index* idx) noexcept(false)
{
    FN_CATCH(delete(idx);)
}

EXPORT_API(int) FN_Dimension(faiss::Index* idx) noexcept(false)
{
    FN_CATCH(return idx->d;)
}

EXPORT_API(faiss::MetricType) FN_MetricType(faiss::Index* idx) noexcept(false)
{
    FN_CATCH(return idx->metric_type;)
}

EXPORT_API(faiss::idx_t) FN_Count(faiss::Index* idx) noexcept(false)
{
     FN_CATCH(return idx->ntotal;)
}

EXPORT_API(const char*) FN_GetLastError()
{
    return LastError.c_str();
}
