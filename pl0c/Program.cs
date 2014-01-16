using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace pl0c {
    class Program {
        //initialize the program
        static void Main(string[] args) {
            //path to input file
            string input_file = "";
            //path to output file
            string output_file = "";
            //path to intermediate language file
            string inter_lang_file = "";
            //
            bool trace_switch = false;
            //
            bool optimize_switch = false;
            //trace a lot of infomation
            bool is_verbose = false;
            //
            bool yes_to_all = false;
            //
            bool show_result = false;
            //
            bool keep_asm = false;
            //
            bool very_verbose = false;

            Console.WriteLine("A PL/0 Compiler");
            Console.WriteLine(".Net Framework version " + Environment.Version.ToString());

            if (!File.Exists("FASM.EXE") || !File.Exists(@".\INCLUDE\WIN32A-MOD.INC")) {
                error.error_process(error_level.fatal_error, "missing necessary file(s), exiting. ");
            }
            Process p = new Process();
            p.StartInfo.FileName = "FASM.EXE";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            string str = p.StandardOutput.ReadLine();

            p.WaitForExit();

            Console.WriteLine(str.Replace("  "," ") + "\n");


            Queue<string> _arg_queue = new Queue<string>(args);
            string arg = "";

            if (_arg_queue.Count == 0) {
            
                _arg_queue.Enqueue("-h");

            } else {
                if (_arg_queue.Peek() != "-h" && _arg_queue.Peek() != "--help") {
                    arg = _arg_queue.Dequeue();
                    if (!File.Exists(arg)) {
                        error.error_process(error_level.fatal_error, "input file \"" + arg + "\" not exist.");
                    } else {
                        input_file = Path.GetFullPath(arg);
                    }
                }
            }         

            while (_arg_queue.Count > 0) {
                arg = _arg_queue.Dequeue();
                if (arg == "-o" || arg == "--out") {
                    try {
                        arg = _arg_queue.Dequeue();
                        output_file = Path.GetFullPath(arg);
                    } catch {
                        error.error_process(error_level.fatal_error, "wrong output file name.");
                    }
                } else if (arg == "-m" || arg == "--intermediate") {
                    try {
                        arg = _arg_queue.Dequeue();
                        inter_lang_file = Path.GetFullPath(arg);
                    } catch {
                        error.error_process(error_level.fatal_error, "wrong intermediate language file name.");
                    }
                } else if (arg == "-t" || arg == "--trace") {

                    trace_switch = true;

                } else if (arg == "-O" || arg == "--optimize") {

                    optimize_switch = true;

                } else if (arg == "-y" || arg == "--yes") {

                    yes_to_all = true;

                } else if (arg == "-v" || arg == "--verbose") {

                    is_verbose = true;
                    show_result = true;

                } else if (arg == "-r" || arg == "--result") {

                    show_result = true;

                } else if (arg == "-a" || arg == "--asm") {

                    keep_asm = true;

                } else if (arg == "-vv" || arg == "--very-verbose") {

                    very_verbose = true;
                    is_verbose = true;
                    show_result = true;

                } else if (arg == "-h" || arg == "--help") {
                    //show help
                    Console.WriteLine("pl0c input_file [options]");
                    Console.WriteLine("Compile options:");
                    Console.WriteLine("  -o bin_file   specify binary executable file (same as --out)");
                    Console.WriteLine("  -m il_file    generate intermediate language file (same as --intermediate)");
                    Console.WriteLine("  -a            keep assembly files (same as --asm)");
                    Console.WriteLine("  -t            trace variant values at the end of the output executable file (same as --trace)");
                    Console.WriteLine("  -O            optimize switch (same as --optimize)");
                    Console.WriteLine("\nAid Options:");
                    Console.WriteLine("  -y            yes to all (same as --yes)");
                    Console.WriteLine("  -v            verbose mode (debugging, same as --verbose)");
                    Console.WriteLine("  -vv           very verbose mode (debugging, same as --very-verbose)");
                    Console.WriteLine("  -r            trace variant values of the input script (forced in verbose mode, same as --result)");
                    Console.WriteLine("  -h            print this message (same as --help)");
                    Environment.Exit(0);
                } else {
                    error.error_process(error_level.fatal_error, "Unknown Option: \"" + arg + "\".", false);
                    Console.WriteLine("Use pl0c -h to show help information.");
                    Environment.Exit(1);
                }
            }

            main_proc main = new main_proc(input_file, output_file, inter_lang_file, trace_switch, optimize_switch, is_verbose, yes_to_all, show_result, keep_asm, very_verbose);

        }
    }
}
