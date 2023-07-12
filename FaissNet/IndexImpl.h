#pragma once
#include <vcclr.h>
#include "Index.h"
#include "IdMap.h"
#include <faiss/Index.h>
#include <faiss/IndexFlat.h>
#include <faiss/index_io.h>
#include <faiss/IndexHNSW.h>

using namespace System;
using namespace System::Runtime::InteropServices;
using size_t = unsigned long long;

namespace FaissNet {
    public ref class Instance {
    public:
        static void WriteIndex(FaissNet::Index^ idx, String^ path) {
            auto ip = Marshal::StringToHGlobalAnsi(path);            
            auto p2 = static_cast<const char*>(ip.ToPointer());
            faiss::write_index(static_cast<faiss::Index*>(idx->Handle()->Ptr()), p2 );
            Marshal::FreeHGlobal(ip);
        }

        generic<class T>
        where T : FaissNet::Index
        static T ReadIndex(String ^ path) {
            auto ip = Marshal::StringToHGlobalAnsi(path);            
            auto p2 = static_cast<const char*>(ip.ToPointer());
            auto idx = faiss::read_index(p2);
            Marshal::FreeHGlobal(ip);
            auto iPdx = gcnew FaissSafeHandle<faiss::Index>(idx);            
            auto args = gcnew array<Object^>(1);
            args[0] = iPdx;
            return static_cast<T>(Activator::CreateInstance(T::typeid, args));            
        }

        static void WriteIdMap(FaissNet::IdMap^ idx, String^ path) {
            auto ip = Marshal::StringToHGlobalAnsi(path);            
            auto p2 = static_cast<const char*>(ip.ToPointer());
            faiss::write_index(static_cast<faiss::Index*>(idx->Handle()->Ptr()), p2 );
            Marshal::FreeHGlobal(ip);
        }

        static IdMap^ ReadIdMap(String^ path) {
            auto ip = Marshal::StringToHGlobalAnsi(path);            
            auto p2 = static_cast<const char*>(ip.ToPointer());
            auto idx = faiss::read_index(p2);
            Marshal::FreeHGlobal(ip);
            auto idIdx = static_cast<faiss::IndexIDMap2*>(idx);
            auto h = gcnew FaissSafeHandle<faiss::IndexIDMap2>(idIdx);
            return gcnew IdMap(h);
        }

    private:
        Instance() {}
    };

    public ref class IndexFlat : public Index {
    public:
        IndexFlat(FaissSafeHandle<faiss::Index>^ ptr) : Index(ptr) {};
        IndexFlat(long dimension, FaissNet::MetricType metric) : 
            Index(gcnew FaissSafeHandle<faiss::Index>(new faiss::IndexFlat(dimension, static_cast<faiss::MetricType>(metric)))) {};

    };

    public ref class IndexFlatL2 : public Index {
    public:
        IndexFlatL2(FaissSafeHandle<faiss::Index>^ ptr) : Index(ptr) {};
        IndexFlatL2(long dimension, FaissNet::MetricType metric) : 
            Index(gcnew FaissSafeHandle<faiss::Index>(new faiss::IndexFlatL2(dimension))) {};
    };

    public ref class IndexHNSW abstract: public Index  {
    public:
        IndexHNSW(FaissSafeHandle<faiss::Index>^ ptr) : Index(ptr) {};

        void shrink_level_0_neighbors(int size) {
            auto idx = static_cast<faiss::IndexHNSW*>(h());
            idx->shrink_level_0_neighbors(size);
        }

        /// reorder links from nearest to farthest
        void reorder_links() {
            auto idx = static_cast<faiss::IndexHNSW*>(h());
            idx->reorder_links();
        }

        void link_singletons() {
            auto idx = static_cast<faiss::IndexHNSW*>(h());
            idx->link_singletons();

        }
    };

    public ref class IndexHNSWFlat : public IndexHNSW {
    public:
        IndexHNSWFlat(FaissSafeHandle<faiss::Index>^ ptr) : IndexHNSW(ptr) {};
        IndexHNSWFlat(long dimension, long M, FaissNet::MetricType metric) :
            IndexHNSWFlat(gcnew FaissSafeHandle<faiss::Index>(new faiss::IndexHNSWFlat(dimension, M, static_cast<faiss::MetricType>(metric)))) {};
    };
}


