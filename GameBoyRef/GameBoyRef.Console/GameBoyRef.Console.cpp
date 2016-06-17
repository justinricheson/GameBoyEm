#include "stdafx.h"
#include "core.h"

int main()
{
	uint8_t *mem = new uint8_t[128];
	memset(mem, 9, 128);
	auto core = gameboy::Core(mem);
	core.emulateCycle();

	auto state = core.getCpuState();
	auto a = state.getA();

    return 0;
}

