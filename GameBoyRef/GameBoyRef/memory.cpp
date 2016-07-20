#include "memory.h"

#include <cmath>
#include <cstdlib>
#include <cstring>
#include <vector>
#include "memoryrecord.h"

namespace gameboy {
	Memory::Memory()
	{
		mem = std::map<uint16_t, uint8_t>();
	}

	Memory::~Memory() {
	}

	std::vector<MemoryRecord>* Memory::getMemoryRecord() {
		auto record = new std::vector<MemoryRecord>();
		for (auto it = mem.begin(); it != mem.end(); ++it)
		{
			record->push_back(MemoryRecord{ it->first, it->second });
		}

		return record;
	}

	void Memory::setMemoryRecord(std::vector<MemoryRecord> *record) {
		initMem = *record;

		for (auto it = record->begin(); it != record->end(); ++it) {
			mem[it->address] = it->value;
		}
	}

	uint8_t Memory::read(uint16_t address) {
		if (mem.count(address) == 0) {
			return 0; // Only writes allocate memory
		}

		return mem[address];
	}

	void Memory::write(uint16_t address, uint8_t value) {
		mem[address] = value;
	}

	uint16_t Memory::readW(uint16_t address) {
		uint8_t hi = read(address);
		uint8_t lo = read(address + 1);
		return (hi << 8) | lo;
	}

	void Memory::writeW(uint16_t address, uint16_t value) {
		mem[address] = (value & 0xFF00) >> 8;
		mem[address + 1] = value & 0xFF;
	}
}
