# FaissNet Build

## Requirements
- Visual Studio with C++/cmake support to build FaissNetNative project.
- Intel oneAPI for MKL support
- Visual Studio Build Tools with C++ support (for faiss.lib)

## Build faissLib
Clone faiss (https://github.com/facebookresearch/faiss).

Restore packages for the 'FaissNet' Visual Studio solution so that the MLK nuget package - referenced by the MklHook project - is downloaded.
Do this before building faiss.lib.

Open a 'Native Tools' command prompt and cd to the faiss repo. 
Set the MKL_PATH variable in the script below to point to the 'redist' 'native' folder under the downloaded MKL nuget package,
and execute it from the Native Tools prompt.

```powershell
set MKL_PATH=E:\s\repos\Faiss.Net\packages\intelmkl.redist.win-x64.2023.1.0.46356\runtimes\win-x64\native

cmake -Wno-dev -DFAISS_ENABLE_GPU=OFF -DFAISS_ENABLE_PYTHON=OFF -DFAISS_ENABLE_C_API=OFF -DBUILD_TESTING=OFF -DBLA_VENDOR=Intel10_64_dyn "-DMKL_LIBRARIES=%MKL_PATH%/mkl_intel_lp64_dll.lib;%MKL_PATH%/mkl_intel_thread_dll.lib;%MKL_PATH%/mkl_core_dll.lib" -DBUILD_SHARED_LIBS=OFF -B build -S .

cmake --build build --config Release --target faiss
cmake --build build --config Debug --target faiss
```
The faiss.lib outputs should be in *'[faiss repo]/build/faiss/[Debug | Release]'* folders.

## Build FaissNetNative (Windows)
FaissNetNative is a Visual Studio c++ 'cmake' project. 

Open the folder *'[FaissNet repo directory]/FaissNetNative'* in Visual Studio.
Review and update the paths for FAISS_DIR, MKL_DIR, MKL_INC in *'/FaissNetNative/CMakeLists.txt*. 
Note: This is **not** the CMakeLists.txt in the project directory root.

In Solution Explorer switch to "CMake Targets View" using the context menu. In the 'targets view', use the context menu again to:

- 'Build all' FaissNetNative
- 'Install' FaissNetNative (this copies the output to a location for nuget packaging)

### FassNetNative for other platforms
Since FaissNetNative is a 'cmake' project, it can be built for MACOS and Linux, also. Please see Visual Studio cmake documentation for details.

## Nuget Package
In Visual Studio, right-click on 'FaissNet' project and select 'Pack' (or use the appropriate dotnet pack command)


