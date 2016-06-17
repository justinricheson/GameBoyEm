#pragma once

#include <cinttypes>
#include "memoryrecord.h"

namespace gameboy {
	class Memory {
	public:
		explicit Memory(uint8_t *initMem);
		virtual ~Memory();

		uint8_t read(uint16_t address);
		void write(uint16_t address, uint8_t value);
		uint16_t readW(uint16_t address);
		void writeW(uint16_t address, uint16_t value);
		MemoryRecord *getMemoryRecord();

	private:
		uint8_t *mem;
		uint8_t next;
		uint8_t nextRecord;
		MemoryRecord *records;
	};
}