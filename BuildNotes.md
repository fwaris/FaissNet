# FaissNet Build

## Requirements
- Visual Studio with C++/ClI support to build FaissNet project.
- Intel oneAPI with Visual Studio support to link against MKL.
- Visual Studio Build Tools with C++ support (for faiss.lib)

## Build faissLib
Clone faiss (https://github.com/facebookresearch/faiss).

Restore packages for solution so that the MLK nuget package - referenced by the MklHook project - is downloaded.

Open a 'Native Tools' command prompt and cd to faiss repo. 
Set the MKL_PATH variable in the script below to point to the 'redist' 'native' folder under the downloaded MKL nuget package,
and execute it from the Native Tools prompt.

```powershell
set MKL_PATH=E:\s\repos\Faiss.Net\packages\intelmkl.redist.win-x64.2023.1.0.46356\runtimes\win-x64\native

cmake -Wno-dev -DFAISS_ENABLE_GPU=OFF -DFAISS_ENABLE_PYTHON=OFF -DFAISS_ENABLE_C_API=OFF -DBUILD_TESTING=OFF -DBLA_VENDOR=Intel10_64_dyn "-DMKL_LIBRARIES=%MKL_PATH%/mkl_intel_lp64_dll.lib;%MKL_PATH%/mkl_intel_thread_dll.lib;%MKL_PATH%/mkl_core_dll.lib" -DBUILD_SHARED_LIBS=OFF -B build -S .

cmake --build build --config Release --target faiss
cmake --build build --config Debug --target faiss

```

The faiss.lib outputs should be in '<faiss repo>/build/faiss/[Debug | Release]' folders.

## Build FaissNet
Ensure that FaissNet Visuals Studio project has the correct paths to the faiss header files and links to the faiss.lib files
for both debug and release configurations.

Build FaissNet and run TestFaissX.

## Nuget Package
In Visual Studio, right-click on 'FaissNetPack' project and select 'Pack' (or use the appropriate dotnet pack command)


