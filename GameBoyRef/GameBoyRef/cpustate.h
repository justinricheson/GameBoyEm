#pragma once

#ifdef GAMEBOYREF_EXPORTS
#define GAMEBOY_API __declspec(dllexport) 
#else
#define GAMEBOY_API __declspec(dllimport) 
#endif

#include "memoryrecord.h"
#include "cpuregisters.h"

namespace gameboy {
	class GAMEBOY_API CpuState {
	public:
		explicit CpuState(MemoryRecord *records, CPURegisters *registers);

		uint8_t getA() const;
		uint8_t getB() const;
		uint8_t getC() const;
		uint8_t getD() const;
		uint8_t getE() const;
		uint8_t getF() const;
		uint8_t getH() const;
		uint8_t getL() const;

		bool getZeroFlag() const;
		bool getSubFlag() const;
		bool getHalfCarryFlag() const;
		bool getCarryFlag() const;

		bool getIME() const;
		uint16_t getSP() const;
		uint16_t getPC() const;

		MemoryRecord *getMemoryRecord() const;

	private:
		MemoryRecord *memoryRecords;
		CPURegisters *cpuRegisters;
	};
}