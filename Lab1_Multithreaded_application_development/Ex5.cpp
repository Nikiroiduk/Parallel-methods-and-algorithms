#include <cstring>

#include "GenericArray.h"
#include "Ex5.h"

const int MAX_THREADS = 4;
const int COLLECTION_SIZE_STEP = 500;
const int NUMBER_OF_STEPS = 200;

int main(int argc, char *argv[])
{
    bool pyFlag = false;
    bool runOnce = false;

    for (int i = 1; i < argc; ++i)
    {
        if (std::strcmp(argv[i], "-py") == 0)
        {
            pyFlag = true;
            break;
        }
        if (std::strcmp(argv[i], "-once") == 0)
        {
            runOnce = true;
            break;
        }
    }
    if (runOnce)
    {
        calc(NUMBER_OF_STEPS, pyFlag);
    }
    else
    {
        for (int i = 1; i <= NUMBER_OF_STEPS; ++i)
        {
            calc(i, pyFlag);
        }
    }

    return 0;
}

void calc(int i, bool pyFlag)
{
    GenericArray<int> arr(COLLECTION_SIZE_STEP * i, pyFlag);
    arr.generateWorstCase();
    arr.sort();
    for (int threads = 0; threads < MAX_THREADS; ++threads){
        arr.generateWorstCase();
        arr.sortMultiThreaded(threads);
    }
    for (int threads = 0; threads < MAX_THREADS; ++threads){
        arr.generateWorstCase();
        arr.sortOMP(threads);
    }
    if (!pyFlag)
        std::cout << "\n";
}
