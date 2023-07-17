#include "pch.h"
#include "TWrap.h"

ref class MyClass {
public:
    int test_run() {
        return 0;
    }

    // nested interface
    interface struct MyInterface {
        void f();
    };
};