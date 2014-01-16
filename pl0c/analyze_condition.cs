using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pl0c {
    class analyze_condition {

        internal static bool has_program_header = false;
        internal static bool has_constant_def = false;
        internal static bool in_constant_def_field = false;
        internal static bool has_variant_def = false;
        internal static bool in_variant_def = false;
        internal static bool has_multi_statement_field = false;
        internal static int multi_statement_level = 0;

        internal static int next_quaternion_line_no = 0;
        internal static int latest_temporay_variant_no = 0;

        internal static List<quaternion> quaternion_list = new List<quaternion>();
        internal static List<symbol> symbol_table = new List<symbol>();

    }
}
