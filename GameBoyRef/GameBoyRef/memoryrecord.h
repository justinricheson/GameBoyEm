#pragma once

#include <cinttypes>

namespace gameboy {
	struct MemoryRecord {
		uint8_t recordtype;
		uint16_t address;
		uint16_t value;
	};
}