#include "stdafx.h"
#include "functions.h"

int main()
{
	char *input = "1|2|3|4|5|6|7|8|0|0|1|0|1|0,0:39|1:1|2:2";
	char *output = new char[1024];
	auto result = Run(input, output);

	return 0;
}

