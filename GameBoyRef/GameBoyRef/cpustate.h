#pragma once

#ifdef GAMEBOYREF_EXPORTS
#define GAMEBOY_API __declspec(dllexport) 
#else
#define GAMEBOY_API __declspec(dllimport) 
#endif

#include "memoryrecord.h"
#include "cpuregisters.h"

namespace gameboy {
	struct GAMEBOY_API CpuState {
		uint8_t A;
		uint8_t B;
		uint8_t C;
		uint8_t D;
		uint8_t E;
		uint8_t F;
		uint8_t H;
		uint8_t L;
		uint16_t SP;
		uint16_t PC;
		bool FZ;
		bool FN;
		bool FH;
		bool FC;
		bool IME;
		MemoryRecord *MemoryRecord;
	};
}