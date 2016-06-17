#include "functions.h"

#include <string>
#include <sstream>
#include <iterator>
#include "core.h"
#include "memory.h"

const int Run(char *input, char *output) {
	uint8_t *initMem = new uint8_t[128];
	memset(initMem, 0, 128);

	auto core = gameboy::Core(initMem);
	core.emulateCycle();

	std::ostringstream cpuState;
	cpuState
		<< (int)core.registers->getA() << "|"
		<< (int)core.registers->getB() << "|"
		<< (int)core.registers->getC() << "|"
		<< (int)core.registers->getD() << "|"
		<< (int)core.registers->getE() << "|"
		<< (int)core.registers->getF() << "|"
		<< (int)core.registers->getH() << "|"
		<< (int)core.registers->getL() << "|"
		<< std::to_string(core.registers->getSP()) << "|"
		<< std::to_string(core.registers->pc) << "|"
		<< (bool)core.registers->getZeroFlag() << "|"
		<< (bool)core.registers->getSubFlag() << "|"
		<< (bool)core.registers->getHalfCarryFlag() << "|"
		<< (bool)core.registers->getCarryFlag() << "|"
		<< (bool)core.registers->getIME() << ":::";

	auto mem = core.memory->getMemoryRecord();
	for (int i = 0; i < core.memory->nextRecord; i++) {
		auto rec = mem + i;
		cpuState
			<< (int)rec->recordtype << "|"
			<< std::to_string(rec->address) << "|"
			<< std::to_string(rec->value) << ":";
	}

	auto str = cpuState.str();
	auto i = stdext::checked_array_iterator<char*>(output, str.length());
	std::copy(str.begin(), str.end(), i);
	output[str.length()] = '\0';

	return 1;
}