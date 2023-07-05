#include <cmath>
#include <cstdio>
#include <cstdlib>
#include <random>

#include <ctime>

#include <faiss/IndexFlat.h>
#include <faiss/IndexIVFPQ.h>
#include <faiss/IndexPQ.h>
#include <faiss/index_io.h>
#include <faiss/IndexNNDescent.h>

#include <chrono>
//#include "pch.h"

using namespace std::chrono;

double elapsed() {
    time_t tv;
    struct tm* tm;
    time(&tv);
    tm = localtime(&tv);
    return tm->tm_sec + 1e-6;
}

auto indexFn = "E:/s/tmp/populated_index.faissindex";

int main_nndescent(void) {
    // dimension of the vectors to index
    int d = 64;
    int K = 64;

    // size of the database we plan to index
    size_t nb = 10000;

    std::mt19937 rng(12345);

    // make the index object and train it
    faiss::IndexNNDescentFlat index(d, K, faiss::METRIC_L2);
    index.nndescent.S = 10;
    index.nndescent.R = 32;
    index.nndescent.L = K;
    index.nndescent.iter = 10;
    index.verbose = true;

    // generate labels by IndexFlat
    faiss::IndexFlat bruteforce(d, faiss::METRIC_L2);

    std::vector<float> database(nb * d);
    for (size_t i = 0; i < nb * d; i++) {
        database[i] = rng() % 1024;
    }

    { // populating the database
        index.add(nb, database.data());
        bruteforce.add(nb, database.data());
    }

    size_t nq = 1000;

    { // searching the database
        printf("Searching ...\n");
        index.nndescent.search_L = 50;

        std::vector<float> queries(nq * d);
        for (size_t i = 0; i < nq * d; i++) {
            queries[i] = rng() % 1024;
        }

        int k = 5;
        std::vector<faiss::idx_t> nns(k * nq);
        std::vector<faiss::idx_t> gt_nns(k * nq);
        std::vector<float> dis(k * nq);

        auto start = high_resolution_clock::now();
        index.search(nq, queries.data(), k, dis.data(), nns.data());
        auto end = high_resolution_clock::now();

        // find exact kNNs by brute force search
        bruteforce.search(nq, queries.data(), k, dis.data(), gt_nns.data());

        int recalls = 0;
        for (size_t i = 0; i < nq; ++i) {
            for (int n = 0; n < k; n++) {
                for (int m = 0; m < k; m++) {
                    if (nns[i * k + n] == gt_nns[i * k + m]) {
                        recalls += 1;
                    }
                }
            }
        }
        float recall = 1.0f * recalls / (k * nq);
        auto t = duration_cast<microseconds>(end - start).count();
        int qps = nq * 1.0f * 1000 * 1000 / t;

        printf("Recall@%d: %f, QPS: %d\n", k, recall, qps);
    }

    return 0;
}


int demo_ivfpq_indexing_main() {
    double t0 = elapsed();

    // dimension of the vectors to index
    int d = 128;

    // size of the database we plan to index
    size_t nb = 200 * 1000;

    // make a set of nt training vectors in the unit cube
    // (could be the database)
    size_t nt = 100 * 1000;

    // make the index object and train it
    faiss::IndexFlatL2 coarse_quantizer(d);

    // a reasonable number of centroids to index nb vectors
    int ncentroids = int(4 * sqrt(nb));

    // the coarse quantizer should not be dealloced before the index
    // 4 = nb of bytes per code (d must be a multiple of this)
    // 8 = nb of bits per sub-code (almost always 8)
    faiss::IndexIVFPQ index(&coarse_quantizer, d, ncentroids, 4, 8);

    std::mt19937 rng;

    { // training
        printf("[%.3f s] Generating %ld vectors in %dD for training\n",
            elapsed() - t0,
            nt,
            d);

        std::vector<float> trainvecs(nt * d);
        std::uniform_real_distribution<> distrib;
        for (size_t i = 0; i < nt * d; i++) {
            trainvecs[i] = distrib(rng);
        }

        printf("[%.3f s] Training the index\n", elapsed() - t0);
        index.verbose = true;

        index.train(nt, trainvecs.data());
    }

    { // I/O demo
        const char* outfilename = indexFn;
        printf("[%.3f s] storing the pre-trained index to %s\n",
            elapsed() - t0,
            outfilename);

        write_index(&index, outfilename);
    }

    size_t nq;
    std::vector<float> queries;

    { // populating the database
        printf("[%.3f s] Building a dataset of %ld vectors to index\n",
            elapsed() - t0,
            nb);

        std::vector<float> database(nb * d);
        std::uniform_real_distribution<> distrib;
        for (size_t i = 0; i < nb * d; i++) {
            database[i] = distrib(rng);
        }

        printf("[%.3f s] Adding the vectors to the index\n", elapsed() - t0);

        index.add(nb, database.data());

        printf("[%.3f s] imbalance factor: %g\n",
            elapsed() - t0,
            index.invlists->imbalance_factor());

        // remember a few elements from the database as queries
        int i0 = 1234;
        int i1 = 1243;

        nq = i1 - i0;
        queries.resize(nq * d);
        for (int i = i0; i < i1; i++) {
            for (int j = 0; j < d; j++) {
                queries[(i - i0) * d + j] = database[i * d + j];
            }
        }
    }

    { // searching the database
        int k = 5;
        printf("[%.3f s] Searching the %d nearest neighbors "
            "of %ld vectors in the index\n",
            elapsed() - t0,
            k,
            nq);

        std::vector<faiss::idx_t> nns(k * nq);
        std::vector<float> dis(k * nq);

        index.search(nq, queries.data(), k, dis.data(), nns.data());

        printf("[%.3f s] Query results (vector ids, then distances):\n",
            elapsed() - t0);

        for (int i = 0; i < nq; i++) {
            printf("query %2d: ", i);
            for (int j = 0; j < k; j++) {
                printf("%7ld ", nns[j + i * k]);
            }
            printf("\n     dis: ");
            for (int j = 0; j < k; j++) {
                printf("%7g ", dis[j + i * k]);
            }
            printf("\n");
        }

        printf("note that the nearest neighbor is not at "
            "distance 0 due to quantization errors\n");
    }

    return 0;
}

int demo_imi_pq_main() {
    double t0 = elapsed();

    // dimension of the vectors to index
    int d = 64;

    // size of the database we plan to index
    size_t nb = 1000 * 1000;
    size_t add_bs = 10000; // # size of the blocks to add

    // make a set of nt training vectors in the unit cube
    // (could be the database)
    size_t nt = 100 * 1000;

    //---------------------------------------------------------------
    // Define the core quantizer
    // We choose a multiple inverted index for faster training with less data
    // and because it usually offers best accuracy/speed trade-offs
    //
    // We here assume that its lifespan of this coarse quantizer will cover the
    // lifespan of the inverted-file quantizer IndexIVFFlat below
    // With dynamic allocation, one may give the responsability to free the
    // quantizer to the inverted-file index (with attribute do_delete_quantizer)
    //
    // Note: a regular clustering algorithm would be defined as:
    //       faiss::IndexFlatL2 coarse_quantizer (d);
    //
    // Use nhash=2 subquantizers used to define the product coarse quantizer
    // Number of bits: we will have 2^nbits_coarse centroids per subquantizer
    //                 meaning (2^12)^nhash distinct inverted lists
    //
    // The parameter bytes_per_code is determined by the memory
    // constraint, the dataset will use nb * (bytes_per_code + 8)
    // bytes.
    //
    // The parameter nbits_subq is determined by the size of the dataset to
    // index.
    //
    size_t nhash = 2;
    size_t nbits_subq = 9;
    size_t ncentroids = 1 << (nhash * nbits_subq); // total # of centroids
    int bytes_per_code = 16;

    faiss::MultiIndexQuantizer coarse_quantizer(d, nhash, nbits_subq);

    printf("IMI (%ld,%ld): %ld virtual centroids (target: %ld base vectors)",
        nhash,
        nbits_subq,
        ncentroids,
        nb);

    // the coarse quantizer should not be dealloced before the index
    // 4 = nb of bytes per code (d must be a multiple of this)
    // 8 = nb of bits per sub-code (almost always 8)
    faiss::MetricType metric = faiss::METRIC_L2; // can be METRIC_INNER_PRODUCT
    faiss::IndexIVFPQ index(
        &coarse_quantizer, d, ncentroids, bytes_per_code, 8);
    index.quantizer_trains_alone = true;

    // define the number of probes. 2048 is for high-dim, overkill in practice
    // Use 4-1024 depending on the trade-off speed accuracy that you want
    index.nprobe = 2048;

    std::mt19937 rng;
    std::uniform_real_distribution<> distrib;

    { // training.

        // The distribution of the training vectors should be the same
        // as the database vectors. It could be a sub-sample of the
        // database vectors, if sampling is not biased. Here we just
        // randomly generate the vectors.

        printf("[%.3f s] Generating %ld vectors in %dD for training\n",
            elapsed() - t0,
            nt,
            d);

        std::vector<float> trainvecs(nt * d);
        for (size_t i = 0; i < nt; i++) {
            for (size_t j = 0; j < d; j++) {
                trainvecs[i * d + j] = distrib(rng);
            }
        }

        printf("[%.3f s] Training the index\n", elapsed() - t0);
        index.verbose = true;
        index.train(nt, trainvecs.data());
    }

    // the index can be re-loaded later with
    // faiss::Index * idx = faiss::read_index("/tmp/trained_index.faissindex");
    faiss::write_index(&index, indexFn);

    size_t nq;
    std::vector<float> queries;

    { // populating the database
        printf("[%.3f s] Building a dataset of %ld vectors to index\n",
            elapsed() - t0,
            nb);

        std::vector<float> database(nb * d);
        std::vector<faiss::idx_t> ids(nb);
        for (size_t i = 0; i < nb; i++) {
            for (size_t j = 0; j < d; j++) {
                database[i * d + j] = distrib(rng);
            }
            ids[i] = 8760000000L + i;
        }

        printf("[%.3f s] Adding the vectors to the index\n", elapsed() - t0);

        for (size_t begin = 0; begin < nb; begin += add_bs) {
            size_t end = std::min(begin + add_bs, nb);
            index.add_with_ids(
                end - begin,
                database.data() + d * begin,
                ids.data() + begin);
        }

        // remember a few elements from the database as queries
        int i0 = 1234;
        int i1 = 1244;

        nq = i1 - i0;
        queries.resize(nq * d);
        for (int i = i0; i < i1; i++) {
            for (int j = 0; j < d; j++) {
                queries[(i - i0) * d + j] = database[i * d + j];
            }
        }
    }

    // A few notes on the internal format of the index:
    //
    // - the positing lists for PQ codes are index.codes, which is a
    //    std::vector < std::vector<uint8_t> >
    //   if n is the length of posting list #i, codes[i] has length
    //   bytes_per_code * n
    //
    // - the corresponding ids are stored in index.ids
    //
    // - given a vector float *x, finding which k centroids are
    //   closest to it (ie to find the nearest neighbors) can be done with
    //
    //   faiss::idx_t *centroid_ids = new faiss::idx_t[k];
    //   float *distances = new float[k];
    //   index.quantizer->search (1, x, k, dis, centroids_ids);
    //

    faiss::write_index(&index, indexFn);

    { // searching the database
        int k = 5;
        printf("[%.3f s] Searching the %d nearest neighbors "
            "of %ld vectors in the index\n",
            elapsed() - t0,
            k,
            nq);

        std::vector<faiss::idx_t> nns(k * nq);
        std::vector<float> dis(k * nq);

        index.search(nq, queries.data(), k, dis.data(), nns.data());

        printf("[%.3f s] Query results (vector ids, then distances):\n",
            elapsed() - t0);

        for (int i = 0; i < nq; i++) {
            printf("query %2d: ", i);
            for (int j = 0; j < k; j++) {
                printf("%7ld ", nns[j + i * k]);
            }
            printf("\n     dis: ");
            for (int j = 0; j < k; j++) {
                printf("%7g ", dis[j + i * k]);
            }
            printf("\n");
        }
    }
    return 0;
}

int demo_imi_flat_main() {
    double t0 = elapsed();

    // dimension of the vectors to index
    int d = 64;

    // size of the database we plan to index
    size_t nb = 1000 * 1000;
    size_t add_bs = 10000; // # size of the blocks to add

    // make a set of nt training vectors in the unit cube
    // (could be the database)
    size_t nt = 100 * 1000;

    //---------------------------------------------------------------
    // Define the core quantizer
    // We choose a multiple inverted index for faster training with less data
    // and because it usually offers best accuracy/speed trade-offs
    //
    // We here assume that its lifespan of this coarse quantizer will cover the
    // lifespan of the inverted-file quantizer IndexIVFFlat below
    // With dynamic allocation, one may give the responsability to free the
    // quantizer to the inverted-file index (with attribute do_delete_quantizer)
    //
    // Note: a regular clustering algorithm would be defined as:
    //       faiss::IndexFlatL2 coarse_quantizer (d);
    //
    // Use nhash=2 subquantizers used to define the product coarse quantizer
    // Number of bits: we will have 2^nbits_coarse centroids per subquantizer
    //                 meaning (2^12)^nhash distinct inverted lists
    //
    // The parameter bytes_per_code is determined by the memory
    // constraint, the dataset will use nb * (bytes_per_code + 8)
    // bytes.
    //
    // The parameter nbits_subq is determined by the size of the dataset to
    // index.
    //
    size_t nhash = 2;
    size_t nbits_subq = 9;
    size_t ncentroids = 1 << (nhash * nbits_subq); // total # of centroids
    int bytes_per_code = 16;

    faiss::MultiIndexQuantizer coarse_quantizer(d, nhash, nbits_subq);

    printf("IMI (%ld,%ld): %ld virtual centroids (target: %ld base vectors)",
        nhash,
        nbits_subq,
        ncentroids,
        nb);

    // the coarse quantizer should not be dealloced before the index
    // 4 = nb of bytes per code (d must be a multiple of this)
    // 8 = nb of bits per sub-code (almost always 8)
    faiss::MetricType metric = faiss::METRIC_L2; // can be METRIC_INNER_PRODUCT
    faiss::IndexIVFPQ index(
        &coarse_quantizer, d, ncentroids, bytes_per_code, 8);
    index.quantizer_trains_alone = true;

    // define the number of probes. 2048 is for high-dim, overkill in practice
    // Use 4-1024 depending on the trade-off speed accuracy that you want
    index.nprobe = 2048;

    std::mt19937 rng;
    std::uniform_real_distribution<> distrib;

    { // training.

        // The distribution of the training vectors should be the same
        // as the database vectors. It could be a sub-sample of the
        // database vectors, if sampling is not biased. Here we just
        // randomly generate the vectors.

        printf("[%.3f s] Generating %ld vectors in %dD for training\n",
            elapsed() - t0,
            nt,
            d);

        std::vector<float> trainvecs(nt * d);
        for (size_t i = 0; i < nt; i++) {
            for (size_t j = 0; j < d; j++) {
                trainvecs[i * d + j] = distrib(rng);
            }
        }

        printf("[%.3f s] Training the index\n", elapsed() - t0);
        index.verbose = true;
        index.train(nt, trainvecs.data());
    }


    // the index can be re-loaded later with
    // faiss::Index * idx = faiss::read_index("/tmp/trained_index.faissindex");
    faiss::write_index(&index, indexFn);

    size_t nq;
    std::vector<float> queries;

    { // populating the database
        printf("[%.3f s] Building a dataset of %ld vectors to index\n",
            elapsed() - t0,
            nb);

        std::vector<float> database(nb * d);
        std::vector<faiss::idx_t> ids(nb);
        for (size_t i = 0; i < nb; i++) {
            for (size_t j = 0; j < d; j++) {
                database[i * d + j] = distrib(rng);
            }
            ids[i] = 8760000000L + i;
        }

        printf("[%.3f s] Adding the vectors to the index\n", elapsed() - t0);

        for (size_t begin = 0; begin < nb; begin += add_bs) {
            size_t end = std::min(begin + add_bs, nb);
            index.add_with_ids(
                end - begin,
                database.data() + d * begin,
                ids.data() + begin);
        }

        // remember a few elements from the database as queries
        int i0 = 1234;
        int i1 = 1244;

        nq = i1 - i0;
        queries.resize(nq * d);
        for (int i = i0; i < i1; i++) {
            for (int j = 0; j < d; j++) {
                queries[(i - i0) * d + j] = database[i * d + j];
            }
        }
    }

    // A few notes on the internal format of the index:
    //
    // - the positing lists for PQ codes are index.codes, which is a
    //    std::vector < std::vector<uint8_t> >
    //   if n is the length of posting list #i, codes[i] has length
    //   bytes_per_code * n
    //
    // - the corresponding ids are stored in index.ids
    //
    // - given a vector float *x, finding which k centroids are
    //   closest to it (ie to find the nearest neighbors) can be done with
    //
    //   faiss::idx_t *centroid_ids = new faiss::idx_t[k];
    //   float *distances = new float[k];
    //   index.quantizer->search (1, x, k, dis, centroids_ids);
    //

    faiss::write_index(&index, indexFn);

    { // searching the database
        int k = 5;
        printf("[%.3f s] Searching the %d nearest neighbors "
            "of %ld vectors in the index\n",
            elapsed() - t0,
            k,
            nq);

        std::vector<faiss::idx_t> nns(k * nq);
        std::vector<float> dis(k * nq);

        index.search(nq, queries.data(), k, dis.data(), nns.data());

        printf("[%.3f s] Query results (vector ids, then distances):\n",
            elapsed() - t0);

        for (int i = 0; i < nq; i++) {
            printf("query %2d: ", i);
            for (int j = 0; j < k; j++) {
                printf("%7ld ", nns[j + i * k]);
            }
            printf("\n     dis: ");
            for (int j = 0; j < k; j++) {
                printf("%7g ", dis[j + i * k]);
            }
            printf("\n");
        }
    }
    return 0;
}

int main(void) {
   demo_imi_flat_main();
   demo_imi_pq_main();
   demo_ivfpq_indexing_main();
   return 0;
}