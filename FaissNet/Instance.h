#pragma once
#include <vcclr.h>
#include "Index.h"
#include <faiss/Index.h>
#include <faiss/index_io.h>

using namespace System;
using namespace System::Runtime::InteropServices;
using size_t = unsigned long long;

namespace FaissNet {
    public ref class Instance {
    public:
        static void WriteIndex(FaissNet::Index^ idx, String^ path) {
            auto ip = Marshal::StringToHGlobalAnsi(path);
            auto p2 = static_cast<const char*>(ip.ToPointer());
            faiss::write_index(static_cast<faiss::Index*>(idx->Handle()->Ptr()), p2);
            Marshal::FreeHGlobal(ip);
        }

        static FaissNet::Index^ ReadIndex(String^ path) {
            auto ip = Marshal::StringToHGlobalAnsi(path);
            auto p2 = static_cast<const char*>(ip.ToPointer());
            auto idx = faiss::read_index(p2);
            Marshal::FreeHGlobal(ip);
            auto iPdx = gcnew FaissSafeHandle<faiss::Index>(idx);
            return gcnew Index(iPdx);
        }

    private:
        Instance() {}
    };
}