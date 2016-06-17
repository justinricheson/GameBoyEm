#include "stdafx.h"
#include "functions.h"

int main()
{
	char *input = new char[1024];
	char *output = new char[1024];
	auto result = Run(input, output);

	return 0;
}

