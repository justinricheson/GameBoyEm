#include "memory.h"

#include <cmath>
#include <cstdlib>
#include <cstring>
#include "memoryrecord.h"

namespace gameboy {
	Memory::Memory(uint8_t *initMem)
	{
		next = 0;
		nextRecord = 0;
		mem = initMem;
		records = new MemoryRecord[128];
	}

	Memory::~Memory() {
		delete[] mem;

		/*for (int i = 0; i<128; i++)
			delete records[i];*/
		delete[] records;
	}

	MemoryRecord* Memory::getMemoryRecord() {
		return records;
	}

	uint8_t Memory::read(uint16_t address) {
		records[nextRecord++] = MemoryRecord{ 0, address };
		uint8_t *memLoc = mem + next++;
		return *memLoc;
	}

	void Memory::write(uint16_t address, uint8_t value) {
		records[nextRecord++] = MemoryRecord{ 1, address, value };
	}

	uint16_t Memory::readW(uint16_t address) {
		records[nextRecord++] = MemoryRecord{ 2, address };

		uint8_t *hi = mem + next + 1;
		uint8_t *lo = mem + next;
		int memLoc = (*hi << 8) | *lo;
		next += 2;
		return memLoc;
	}

	void Memory::writeW(uint16_t address, uint16_t value) {
		records[nextRecord++] = MemoryRecord{ 3, address, value };
	}
}
