#include "cpustate.h"

#include "memoryrecord.h"
#include "cpuregisters.h"

namespace gameboy {
	CpuState::CpuState(MemoryRecord *records, CPURegisters *registers) {
		memoryRecords = records;
		cpuRegisters = registers;
	}

	uint8_t CpuState::getA() const {
		return cpuRegisters->getA();
	}

	uint8_t CpuState::getB() const {
		return cpuRegisters->getB();
	}

	uint8_t CpuState::getC() const {
		return cpuRegisters->getC();
	}

	uint8_t CpuState::getD() const {
		return cpuRegisters->getD();
	}

	uint8_t CpuState::getE() const {
		return cpuRegisters->getE();
	}

	uint8_t CpuState::getF() const {
		return cpuRegisters->getF();
	}

	uint8_t CpuState::getH() const {
		return cpuRegisters->getH();
	}

	uint8_t CpuState::getL() const {
		return cpuRegisters->getL();
	}

	bool CpuState::getZeroFlag() const {
		return cpuRegisters->getZeroFlag();
	}

	bool CpuState::getSubFlag() const {
		return cpuRegisters->getSubFlag();
	}

	bool CpuState::getHalfCarryFlag() const {
		return cpuRegisters->getHalfCarryFlag();
	}

	bool CpuState::getCarryFlag() const {
		return cpuRegisters->getCarryFlag();
	}

	bool CpuState::getIME() const {
		return cpuRegisters->getIME();
	}

	uint16_t CpuState::getSP() const {
		return cpuRegisters->getSP();
	}

	uint16_t CpuState::getPC() const {
		return cpuRegisters->pc;
	}

	MemoryRecord * CpuState::getMemoryRecord() const {
		return memoryRecords;
	}
}
