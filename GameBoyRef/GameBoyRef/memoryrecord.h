#pragma once

#include <cinttypes>

namespace gameboy {
	struct MemoryRecord {
		uint16_t address;
		uint8_t value;
	};
}