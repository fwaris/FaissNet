# CMakeList.txt : CMake project for FaissNetNative, include source and define
# project specific logic here.
#
set(FAISS_DIR E:/s/repos/faiss)

set(MKL_DIR ${CMAKE_CURRENT_SOURCE_DIR}/../../packages/intelmkl.devel.win-x64.2023.1.0.46356/lib/native/win-x64)
set(MKL_INC ${CMAKE_CURRENT_SOURCE_DIR}/../../packages/intelmkl.devel.win-x64.2023.1.0.46356/lib/native/include)


include_directories(${FAISS_DIR})
include_directories(${MKL_INCLUDE})


# Add source to this project's executable.
set(SOURCES
	FaissNetNative.h
	FaissNetNative.cpp 
	Stdafx.h)


set(BLA_VENDOR Intel10_64_dyn)
#find_package(BLAS REQUIRED)
set(BUILD_SHARED_LIBS true)

add_library (FaissNetNative SHARED ${SOURCES})
target_link_libraries(FaissNetNative "${MKL_DIR}/mkl_intel_lp64_dll.lib")
target_link_libraries(FaissNetNative "${MKL_DIR}/mkl_intel_thread_dll.lib")
target_link_libraries(FaissNetNative "${MKL_DIR}/mkl_core_dll.lib")
target_link_libraries(FaissNetNative debug ${FAISS_DIR}/build/faiss/Debug/faiss.lib)
target_link_libraries(FaissNetNative optimized ${FAISS_DIR}/build/faiss/Release/faiss.lib)


if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET FaissNetNative PROPERTY CXX_STANDARD 20)
endif()

set(config $<IF:$<CONFIG:Debug>,Debug,Release>)
set(targetfile ${CMAKE_CURRENT_SOURCE_DIR}/../../x64/${config}/$<TARGET_FILE_NAME:FaissNetNative>)

add_custom_command(TARGET FaissNetNative POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:FaissNetNative> ${targetfile}
)
# TODO: Add tests and install targets if needed.
