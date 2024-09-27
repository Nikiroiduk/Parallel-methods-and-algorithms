#pragma once

#include <iostream>
#include <vector>
#include <chrono>
#include <thread>
#include <mutex>
#include <future>

#include <omp.h>


template <typename T>
class GenericArray
{
private:
	std::vector<T> data;
	int size;
	bool pyFlag;
	void merge(size_t left, size_t mid, size_t right);
	void mergeSort(size_t left, size_t right);
	void mergeSortThreaded(size_t left, size_t right, int maxDepth);
	void mergeSortOMP(size_t left, size_t right);
public:
	GenericArray(size_t n, bool flag);
	void display() const;
	void generateWorstCase();
	void sort();
	void sortMultiThreaded(int threads = 0);
	void sortOMP(int threads = 0);
};

#include "GenericArray.tpp"