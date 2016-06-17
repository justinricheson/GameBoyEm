#include "memory.h"

#include <cmath>
#include <cstdlib>
#include <cstring>

namespace gameboy {
	Memory::Memory(uint8_t *initMem)
	{
		next = 0;
		mem = initMem;
	}

	Memory::~Memory() {
		delete[] mem;
	}

	uint8_t Memory::read(uint16_t address) {
		// TODO LOG address
		uint8_t *memLoc = mem + next++;
		return *memLoc;
	}

	void Memory::write(uint16_t address, uint8_t value) {
		// TODO LOG address and value
	}

	uint16_t Memory::readW(uint16_t address) {
		// TODO LOG address
		auto memLoc = (read(next + 1) << 8) | read(next);
		next += 2;
		return memLoc;
	}

	void Memory::writeW(uint16_t address, uint16_t value) {
		// TODO LOG address and value
	}
}
