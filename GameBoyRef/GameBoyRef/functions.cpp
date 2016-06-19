#include "functions.h"

#include <string>
#include <sstream>
#include <iterator>
#include <vector>
#include <iostream>
#include "core.h"
#include "memory.h"

std::vector<std::string> &split(const std::string &s, char delim, std::vector<std::string> &elems) {
	std::stringstream ss(s);
	std::string item;
	while (std::getline(ss, item, delim)) {
		elems.push_back(item);
	}
	return elems;
}


std::vector<std::string> split(const std::string &s, char delim) {
	std::vector<std::string> elems;
	split(s, delim, elems);
	return elems;
}

bool to_bool(std::string const& s) {
	return s != "0";
}

const int Run(char *input, char *output) {
	auto inputStr = std::string(input);
	auto split1 = split(input, ',');
	auto registers = split(split1[0], '|');
	auto inMem = split(split1[1], '|');
	
	auto *initMem = new std::vector<gameboy::MemoryRecord>();
	for (int i = 0; i < inMem.size(); i++) {
		auto kvp = split(inMem[i], ':');
		initMem->push_back(gameboy::MemoryRecord{
			(uint16_t)std::stoi(kvp[0].c_str()),
			(uint8_t)std::stoi(kvp[1].c_str())
		});
	}

	auto core = gameboy::Core();
	core.memory->setMemoryRecord(initMem);
	core.registers->setA(std::stoi(registers[0].c_str()));
	core.registers->setB(std::stoi(registers[1].c_str()));
	core.registers->setC(std::stoi(registers[2].c_str()));
	core.registers->setD(std::stoi(registers[3].c_str()));
	core.registers->setE(std::stoi(registers[4].c_str()));
	//core.registers->setF(std::stoi(registers[5].c_str())); // Set flags individually
	core.registers->setH(std::stoi(registers[5].c_str()));
	core.registers->setL(std::stoi(registers[6].c_str()));
	core.registers->setSP(std::stoi(registers[7].c_str()));
	core.registers->pc = std::stoi(registers[8].c_str());
	core.registers->setZeroFlag(to_bool(registers[9]));
	core.registers->setSubFlag(to_bool(registers[10]));
	core.registers->setHalfCarryFlag(to_bool(registers[11]));
	core.registers->setCarryFlag(to_bool(registers[12]));
	core.registers->setIME(to_bool(registers[13]));

	core.emulateCycle();

	std::ostringstream cpuState;
	cpuState
		<< (int)core.registers->getA() << "|"
		<< (int)core.registers->getB() << "|"
		<< (int)core.registers->getC() << "|"
		<< (int)core.registers->getD() << "|"
		<< (int)core.registers->getE() << "|"
		//<< (int)core.registers->getF() << "|" // Get flags individually
		<< (int)core.registers->getH() << "|"
		<< (int)core.registers->getL() << "|"
		<< std::to_string(core.registers->getSP()) << "|"
		<< std::to_string(core.registers->pc) << "|"
		<< (bool)core.registers->getZeroFlag() << "|"
		<< (bool)core.registers->getSubFlag() << "|"
		<< (bool)core.registers->getHalfCarryFlag() << "|"
		<< (bool)core.registers->getCarryFlag() << "|"
		<< (bool)core.registers->getIME() << ",";

	auto outMem = core.memory->getMemoryRecord();
	for (int i = 0; i < outMem->size(); i++) {
		gameboy::MemoryRecord rec = outMem->at(i);
		cpuState
			<< std::to_string(rec.address) << ":"
			<< std::to_string(rec.value);

		if (i + 1 < outMem->size())
			cpuState << "|";
	}

	auto str = cpuState.str();
	auto i = stdext::checked_array_iterator<char*>(output, str.length());
	std::copy(str.begin(), str.end(), i);
	output[str.length()] = '\0';

	return 1;
}