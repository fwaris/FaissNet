#pragma once

#include "Index.h"
#include <faiss/index_factory.h>
#include <faiss/impl/FaissException.h>
using namespace System;
namespace FaissNet {
    public ref class Factory {
    public:
        /// <summary>
        /// See: https://github.com/facebookresearch/faiss/wiki/The-index-factory
        /// </summary>
        static Index^ Create(int dimension, String^ description, MetricType metric) {
            auto descH = Marshal::StringToHGlobalAnsi(description);
            auto descP = static_cast<const char*>(descH.ToPointer());
            faiss::index_factory_verbose = 1;
            try {
                faiss::Index* index = faiss::index_factory(dimension, descP, static_cast<faiss::MetricType>(metric));
                auto h = gcnew FaissSafeHandle<faiss::Index>(index);
                return gcnew FaissNet::Index(h);
            }
            catch (const faiss::FaissException& ex) {
                auto msg = gcnew System::String(ex.msg.c_str());
                throw(gcnew System::Exception(msg));
            }
            finally {
                Marshal::FreeHGlobal(descH);
            }
        }

        /// <summary>
        /// Index created with string "IDMap2,HNSW32"
        static Index^ Default(int dimension, MetricType metric) {
            return Factory::Create(dimension, "IDMap2,HNSW32", metric);
        }
    };
}