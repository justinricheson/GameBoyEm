#include "core.h"

#include <cstdlib>
#include "cpuregisters.h"
#include "memory.h"

namespace gameboy {
	Core::Core(uint8_t *initMem) :
		registers(new CPURegisters),
		memory(new Memory(initMem)) {
		conditional = false;
		clock = 0;
	}

	Core::~Core() {
		delete memory;
		delete registers;
	}

	CpuState Core::getCpuState() {
		return CpuState(memory->getMemoryRecord(), registers);
	}

	void Core::emulateCycle() {
		uint8_t lastClocks = 0;
		uint8_t opCode = memory->read(registers->pc++);
		uint8_t cb = memory->read(registers->pc);

		(this->*opCodes[opCode])();

		if (opCode == 0xCB) {
			lastClocks = opCodeCBCycles[cb];
		}
		else if (conditional) {
			conditional = false;
			lastClocks = opCodeCondCycles[opCode];
		}
		else {
			lastClocks = opCodeCycles[opCode];
		}

		clock += lastClocks;
	}

	void Core::handleCB() {
		(this->*opCodesCB[memory->read(registers->pc++)])();
	}

	void Core::xx() {
	}

	void Core::CBxx() {
	}
}
