using System.Collections.Generic;

namespace pl0c {
    /// <summary>
    /// Some important constants
    /// </summary>
    public class C {
        public static readonly List<string> delimiter = new List<string>(){
            " ", "+", "-", "*", "/", ":=", "=", "<>", ">", ">=", "<", "<=", "(", ")", ";", ",","\t"
        };

        public static readonly List<string> op_rel = new List<string>() {
             "=", "<>", ">", ">=", "<", "<="
        };

        public static readonly List<string> reserved_symbol = new List<string>(){
            "PROGRAM", "BEGIN", "END", "CONST", "VAR", "WHILE", "DO", "IF", "THEN"
        };

        public static readonly List<string> data_gpr = new List<string>(){
            "eax","ebx","ecx","edx"
        };

        public static readonly string alphabet = "abcdefghijklmnopqrstuvwxyz";

        public static readonly string number = "0123456789";

    }
}
