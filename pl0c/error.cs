using System;

namespace pl0c {
    internal enum error_level {
        fatal_error,
        normal_error,
        warning,
        information
    }
    internal enum error_type {
        internal_code_error,


        integer_parse_error,
        unrecognized_symbol,
        unnecessary_part_after_program_def,
        missing_delimiter,
        variant_has_been_declared,
        wrong_delimiter,
        no_more_declaration,
        constant_exists,
        grammar_error,
        expression_error,
        unknown_symbol,
        constant_value_assignment,
                
        unknown_error
    }
    class error {
        internal static bool error_has_occurred = false;
        /// <summary>
        /// trace message and exit when error occurred if needed
        /// </summary>
        /// <param name="err_type">error type</param>
        /// <param name="message">show message</param>
        /// <param name="exit_after_error">(default true) exit after error (fatal and normal)</param>
        internal static void error_process(error_level err_type, string message, bool exit_after_fatal_error = true) {
            switch (err_type) {
                case error_level.fatal_error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.Write("Fatal Error: ");
                    break;
                case error_level.normal_error:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Error.Write("Error: ");
                    break;
                case error_level.warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Warning: ");
                    break;
                case error_level.information:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Info: ");
                    break;
            }
            if (err_type < error_level.warning) {
                error.error_has_occurred = true;
                Console.Error.WriteLine(message);
                Console.ResetColor();
                if (exit_after_fatal_error == true && err_type == error_level.fatal_error) {
                    Environment.Exit((int)err_type + 1);
                }
            } else {
                Console.WriteLine(message);
                Console.ResetColor();
            }
        }
    }
}
