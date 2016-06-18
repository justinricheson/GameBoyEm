#include "stdafx.h"
#include "functions.h"

int main()
{
	char *input = "0|0|0|0|0|0|0|0|0|0|0|0|0|0|0,000000";
	char *output = new char[1024];
	auto result = Run(input, output);

	return 0;
}

