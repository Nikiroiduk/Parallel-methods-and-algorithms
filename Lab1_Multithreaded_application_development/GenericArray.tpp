#pragma once

// #include "GenericArray.h"

template <typename T>
GenericArray<T>::GenericArray(size_t n, bool flag)
{
    size = n;
    pyFlag = flag;
    for (T i = 1; i <= n; ++i)
    {
        data.push_back(i);
    }
}

template <typename T>
void GenericArray<T>::display() const
{
    for (const T &item : data)
    {
        std::cout << item << " ";
    }
    std::cout << std::endl;
}

template <typename T>
void GenericArray<T>::generateWorstCase()
{
    for (size_t i = 0; i < size; ++i)
    {
        data[i] = size - i;
    }
}

template <typename T>
void GenericArray<T>::merge(size_t left, size_t mid, size_t right)
{
    size_t n1 = mid - left + 1;
    size_t n2 = right - mid;

    std::vector<T> L(n1);
    std::vector<T> R(n2);

    for (size_t i = 0; i < n1; ++i)
        L[i] = data[left + i];
    for (size_t i = 0; i < n2; ++i)
        R[i] = data[mid + i + 1];

    size_t i = 0, j = 0, k = left;

    while (i < n1 && j < n2)
    {
        if (L[i] <= R[j])
        {
            data[k] = L[i];
            ++i;
        }
        else
        {
            data[k] = R[j];
            ++j;
        }
        ++k;
    }

    while (i < n1)
    {
        data[k] = L[i];
        ++i;
        ++k;
    }

    while (j < n2)
    {
        data[k] = R[j];
        ++j;
        ++k;
    }
}

template <typename T>
void GenericArray<T>::mergeSort(size_t left, size_t right)
{
    if (left < right)
    {
        size_t mid = left + (right - left) / 2;

        mergeSort(left, mid);
        mergeSort(mid + 1, right);
        merge(left, mid, right);
    }
}

// template <typename T>
// void GenericArray<T>::mergeSortThreaded(size_t left, size_t right, int maxDepth) {
//     if (left < right) {
//         size_t mid = left + (right - left) / 2;
//
//         if (maxDepth > 0) {
//             std::thread leftThread(&GenericArray::mergeSortThreaded, this, left, mid, maxDepth - 1);
//             std::thread rightThread(&GenericArray::mergeSortThreaded, this, mid + 1, right, maxDepth - 1);
//
//             leftThread.join();
//             rightThread.join();
//         }
//         else {
//             mergeSort(left, mid);
//             mergeSort(mid + 1, right);
//         }
//
//         merge(left, mid, right);
//     }
// }

template <typename T>
void GenericArray<T>::mergeSortThreaded(size_t left, size_t right, int maxDepth)
{
    if (left < right)
    {
        size_t mid = left + (right - left) / 2;

        // Максимальная глубина рекурсии (количество потоков)
        if (maxDepth > 0)
        {
            // Запуск асинхронной таски по сортировке левого массива
            auto leftFuture = std::async(std::launch::async, &GenericArray::mergeSortThreaded, this, left, mid, maxDepth - 1);
            // Запуск асинхронной таски по сортировке правого массива
            auto rightFuture = std::async(std::launch::async, &GenericArray::mergeSortThreaded, this, mid + 1, right, maxDepth - 1);

            // Ожидание завершения выполнения
            leftFuture.get();
            rightFuture.get();
        }
        else
        {
            // Если максимальная глубина достигнута выполнять синхронно 
            mergeSort(left, mid);
            mergeSort(mid + 1, right);
        }

        merge(left, mid, right);
    }
}

template <typename T>
void GenericArray<T>::mergeSortOMP(size_t left, size_t right)
{
    // Трешхолд чтобы минимизировать оверхед
    if (right - left < 1000)
    {
        mergeSort(left, right);
        return;
    }

    // Массив пополам
    if (left < right)
    {
        size_t mid = left + (right - left) / 2;
    // Таска для сортировки левого массива
#pragma omp task shared(data)
        {
            mergeSort(left, mid);
        }

    // Таска для сортировки правого массива
#pragma omp task shared(data)
        {
            mergeSort(mid + 1, right);
        }
    // Ожидание завершения выполнения задач
#pragma omp taskwait

        // Объндинение двух отсортированных частей
        merge(left, mid, right);
    }
}

template <typename T>
void GenericArray<T>::sort()
{
    auto start = std::chrono::high_resolution_clock::now();
    mergeSort(0, size - 1);
    auto end = std::chrono::high_resolution_clock::now();

    std::chrono::duration<double, std::milli> duration = end - start;

    if (pyFlag)
        std::cout << "sync," << duration.count() << ",1," << size << std::endl;
    else
        std::cout << "Sorting took " << duration.count() << " ms" << std::endl;
}

template <typename T>
void GenericArray<T>::sortMultiThreaded(int threads)
{
    auto start = std::chrono::high_resolution_clock::now();
    const int maxDepth = threads;
    mergeSortThreaded(0, data.size() - 1, maxDepth);
    auto end = std::chrono::high_resolution_clock::now();

    std::chrono::duration<double, std::milli> duration = end - start;

    if (pyFlag)
        std::cout << "multithreaded," << duration.count() << "," << threads + 1 << "," << size << std::endl;
    else
        std::cout << "Multithreaded sorting took " << duration.count() << " ms\t[ threads: " << threads + 1 << " ]\t[ size: " << size << " ]" << std::endl;
}

template <typename T>
void GenericArray<T>::sortOMP(int threads)
{
    auto start = std::chrono::high_resolution_clock::now();

    omp_set_num_threads(threads);

    // Начало региона параллельного выполнения
#pragma omp parallel
    {
        // Только один поток может запустить сортировку всего массива
#pragma omp single
        {
            mergeSortOMP(0, data.size() - 1);
        }
    }

    // #pragma omp parallel
    //     {
    //             mergeSortOMP(0, data.size() - 1);
    //     }

    auto end = std::chrono::high_resolution_clock::now();

    std::chrono::duration<double, std::milli> duration = end - start;

    if (pyFlag)
        std::cout << "omp," << duration.count() << "," << threads + 1 << "," << size << std::endl;
    else
        std::cout << "OpenMP sorting took " << duration.count() << " ms\t[ threads: " << threads + 1 << " ]\t[ size: " << data.size() << " ]" << std::endl;
}