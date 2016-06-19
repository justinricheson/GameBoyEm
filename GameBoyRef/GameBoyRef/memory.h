#pragma once

#include <cinttypes>
#include <vector>
#include <map>
#include "memoryrecord.h"

namespace gameboy {
	class Memory {
	public:
		explicit Memory();
		virtual ~Memory();

		uint8_t read(uint16_t address);
		void write(uint16_t address, uint8_t value);
		uint16_t readW(uint16_t address);
		void writeW(uint16_t address, uint16_t value);
		std::vector<MemoryRecord> *getMemoryRecord();
		void setMemoryRecord(std::vector<MemoryRecord> *record);

	private:
		uint8_t next;
		std::vector<MemoryRecord> initMem;
		std::map<uint16_t, uint8_t> mem;
		int getnext();
	};
}