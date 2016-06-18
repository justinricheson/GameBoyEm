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
	std::vector<std::string> split1 = split(input, ',');
	std::vector<std::string> split2 = split(split1[0], '|');

	uint8_t *initMem = new uint8_t[split1[1].size() / 2];
	for (int i = 0, j = 0; i < split1[1].size(); i += 2, j++) {
		char tmp[2];
		tmp[0] = split1[1].at(i);
		tmp[1] = split1[1].at(i + 1);
		initMem[j] = strtol(tmp, NULL, 16);
	}

	auto core = gameboy::Core(initMem);
	core.registers->setA(std::stoi(split2[0].c_str()));
	core.registers->setB(std::stoi(split2[1].c_str()));
	core.registers->setC(std::stoi(split2[2].c_str()));
	core.registers->setD(std::stoi(split2[3].c_str()));
	core.registers->setE(std::stoi(split2[4].c_str()));
	core.registers->setF(std::stoi(split2[5].c_str()));
	core.registers->setH(std::stoi(split2[6].c_str()));
	core.registers->setL(std::stoi(split2[7].c_str()));
	core.registers->setSP(std::stoi(split2[8].c_str()));
	core.registers->pc = std::stoi(split2[9].c_str());
	core.registers->setZeroFlag(to_bool(split2[10]));
	core.registers->setSubFlag(to_bool(split2[11]));
	core.registers->setHalfCarryFlag(to_bool(split2[12]));
	core.registers->setCarryFlag(to_bool(split2[13]));
	core.registers->setIME(to_bool(split2[14]));

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
		<< (bool)core.registers->getIME() << ",";

	auto mem = core.memory->getMemoryRecord();
	for (int i = 0; i < core.memory->nextRecord; i++) {
		auto rec = mem + i;
		cpuState
			<< (int)rec->recordtype << "|"
			<< std::to_string(rec->address) << "|"
			<< std::to_string(rec->value);

		if (i + 1 < core.memory->nextRecord)
			cpuState << "-";
	}

	auto str = cpuState.str();
	auto i = stdext::checked_array_iterator<char*>(output, str.length());
	std::copy(str.begin(), str.end(), i);
	output[str.length()] = '\0';

	return 1;
}