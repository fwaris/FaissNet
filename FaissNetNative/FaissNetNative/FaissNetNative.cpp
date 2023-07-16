// FaissNetNative.cpp : Defines the entry point for the application.
//

#include "FaissNetNative.h"
#include <faiss/impl/IDSelector.h>

using namespace std;

EXPORT_API(void*) FN_Create(int dimension, const char* description, faiss::MetricType metric) {
    faiss::Index* index = faiss::index_factory(dimension, description, metric);
    return index;
}

EXPORT_API(void*) FN_CreateDefault(int dimension, faiss::MetricType metric)
{
    return FN_Create(dimension, "IDMap2,HNSW32", metric);
}

EXPORT_API(void) FN_WriteIndex(faiss::Index* idx, const char* path)
{
    faiss::write_index(idx, path);
}

EXPORT_API(void*) FN_ReadIndex(const char* path)
{
    return faiss::read_index(path);
}

EXPORT_API(void) FN_Add(faiss::Index* idx, int n, const float* x)
{
    idx->add(n, x);
}

EXPORT_API(void) FN_AddWithIds(faiss::Index* idx, int n, const float* x, const long long* ids)
{
    idx->add_with_ids(n, x, ids);
}

EXPORT_API(void) FN_Search(faiss::Index* idx, int n, const float* x, int k, float* distances, long long* labels)
{
    idx->search(n, x, k, distances, labels);
}

EXPORT_API(void) FN_Assign(faiss::Index* idx, int n, const float* x, long long* labels, int k)
{
    idx->assign(n, x, labels, k);
}

EXPORT_API(void) FN_Train(faiss::Index* idx, int n, const float* x)
{
    idx->train(n, x);
}

EXPORT_API(void) FN_Reset(faiss::Index* idx)
{
    idx->reset();
}

EXPORT_API(void) FN_RemoveIds(faiss::Index* idx, int n, long long* ids)
{
    auto sel = faiss::IDSelectorArray::IDSelectorArray(n, ids);
    idx->remove_ids(sel);
}

EXPORT_API(void) FN_ReconstructBatch(faiss::Index* idx, int n, const long long* ids, float* recons)
{
   // idx->reconstruct_batch(n, ids, recons);
}

EXPORT_API(void) FN_SearchAndReconstruct(faiss::Index* idx, int n, const float* x, int k, float* distances, long long* labels, float* recons)
{
    idx->search_and_reconstruct(n, x, k, distances, labels, recons);
}

EXPORT_API(void) FN_MergeFrom(faiss::Index* idx, faiss::Index* otherIndex, long long add_id)
{
    idx->merge_from(*otherIndex, add_id);
}

