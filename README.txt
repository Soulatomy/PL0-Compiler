PL/0 Compiler
============

This project is a compiler written in C# and developed in Visual Studio 2010. 
It can convert PL/0 code files to intermediate codes, assembly codes and binary executable files.

Usage
---------

You can input the following command line:

		>pl0c input_file [options]

### The main commands and options

		input_file    input the source code file
		-o bin_file   specify binary executable file (same as --out)");
		-m il_file    generate intermediate language file (same as --intermediate)");
		-a            keep assembly files (same as --asm)");
		-t            trace variant values at the end of the output executable file (same as --trace)");
		-O            optimize switch (same as --optimize)");
		-y            yes to all (same as --yes)");
		-v            verbose mode (debugging, same as --verbose)");
		-vv           very verbose mode (debugging, same as --very-verbose)");
		-r            trace variant values of the input script (forced in verbose mode, same as --result)");
		-h            show help information (same as --help)");

### Example

The original code, saved in samples\sample-1:

		PROGRAM simple
		VAR x,y;
		BEGIN
		x:=1;
		y:=2;
		WHILE x<5 DO x:=x+1;
		IF y>0 THEN y:=y-1;
		y:=y+x;
		END

Open cmd.exe, goto the folder, input the following command line:

		pl0c .\samples\sample-1\simple.pl0 -o .\samples\sample-1\simple.exe -m .\samples\sample-1\simple.il -a -t -r -O

Then the original code has been converted to the intermediate codes (simple.il), assembly codes (simple-2012-08-12-06-26-54-7021.asm) and a binary executable file (simple.exe). 

The output of the intermediate codes:

		Program: simple
		000: (mov, x, _i_000, )
		001: (mov, y, _i_001, )
		002: (jl, x, _i_002, 4)
		003: (jmp, , , 7)
		004: (add, x, _i_003, _t_004)
		005: (mov, x, _t_004, )
		006: (jmp, , , 2)
		007: (jg, y, _i_005, 9)
		008: (jmp, , , 11)
		009: (sub, y, _i_006, _t_007)
		010: (mov, y, _t_007, )
		011: (add, y, x, _t_008)
		012: (mov, y, _t_008, )

Related
-----------

[PL/0 language](http://en.wikipedia.org/wiki/PL/0)