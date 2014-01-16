using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pl0c {
    class grammar_analyze_result {
        internal int reduction_length = 0;
        internal bool success = false;
        internal symbol reduction_result = null;
    }
}
