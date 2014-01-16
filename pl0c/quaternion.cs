using System.Text;
using System;

namespace pl0c {
    internal enum quaternion_action {
        jmp,
        je,
        jne,
        jg,
        jge,
        jl,
        jle,
        mov,
        add,
        sub,
        mul,
        div,
        neg
    }
    
    class quaternion {

        internal quaternion_action action = quaternion_action.jmp;
        internal string left = "";
        internal string right = "";
        internal int next = -1;
        internal string result = "";
        internal int id = 0;
        /// <summary>
        /// generate a quaternion
        /// </summary>
        /// <param name="act">action</param>
        /// <param name="l">left</param>
        /// <param name="r">right</param>
        /// <param name="n">next</param>
        internal quaternion(quaternion_action act, string l = "", string r = "", int n = -1) {
            this.action = act;
            this.left = l;
            this.right = r;
            this.next = n;
            this.id = analyze_condition.next_quaternion_line_no++;
        }
        /// <summary>
        /// generate a quaternion
        /// </summary>
        /// <param name="act">action(should not be jump type)</param>
        /// <param name="l">left value</param>
        /// <param name="r">right value</param>
        /// <param name="res">result</param>
        internal quaternion(quaternion_action act, string l, string r, string res) {
            if (act < quaternion_action.mov) {
                Exception ex = new Exception("using wrong .ctor function");
                ex.Data["type"] = error_type.internal_code_error;
                throw ex;
            }
            this.action = act;
            this.left = l;
            this.right = r;
            this.result = res;
            this.id = analyze_condition.next_quaternion_line_no++;
        }

        public override string ToString() {
            StringBuilder sb_result = new StringBuilder(id.ToString("D3"));
            sb_result.Append(": (");
            sb_result.Append(this.action.ToString("G"));
            sb_result.Append(", ");
            sb_result.Append(this.left);
            sb_result.Append(", ");
            sb_result.Append(this.right);
            sb_result.Append(", ");
            if (this.next != -1) {
                sb_result.Append(this.next);
            } else {
                sb_result.Append(this.result);
            }
            sb_result.Append(")");
            return sb_result.ToString();
        }

    }
}
