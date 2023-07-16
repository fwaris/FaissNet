// FaissNetNative.h : Include file for standard system include files,
// or project specific include files.

#pragma once

#include <iostream>
#include "Stdafx.h"
#include <faiss/Index.h>
#include <faiss/index_io.h>
#include <faiss/index_factory.h>

EXPORT_API(void*) FN_Create(int dimension, const char* description, faiss::MetricType metric);
EXPORT_API(void*) FN_CreateDefault(int dimension, faiss::MetricType metric);
EXPORT_API(void) FN_WriteIndex(faiss::Index* idx, const char* path);
EXPORT_API(void*) FN_ReadIndex(const char* path);
EXPORT_API(void) FN_Add(faiss::Index* idx, int n, const float * x);
EXPORT_API(void) FN_AddWithIds(faiss::Index* idx, int n, const float* x, const long long* ids);
EXPORT_API(void) FN_Search(faiss::Index* idx, int n, const float* x, int k, float* distances, long long* labels);
EXPORT_API(void) FN_Assign(faiss::Index* idx, int n, const float* x, long long * labels, int k);
EXPORT_API(void) FN_Train(faiss::Index* idx, int n, const float* x);
EXPORT_API(void) FN_Reset(faiss::Index* idx);
EXPORT_API(void) FN_RemoveIds(faiss::Index* idx,int n, long long* ids);
EXPORT_API(void) FN_ReconstructBatch(faiss::Index* idx,int n, const long long* ids, float* recons);
EXPORT_API(void) FN_SearchAndReconstruct(faiss::Index* idx, int n, const float* x, int k, float* distances, long long* labels, float* recons);
EXPORT_API(void) FN_MergeFrom(faiss::Index* idx, faiss::Index* otherIndex, long long add_id);









