using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pl0c {
    internal enum opcode {
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
        imul,
        idiv,
        neg,
        cmp,
        inc,
        dec,
        cdq,
        push,
        pop,
        xchg,
        sal,
        sar,
        xadd,
        xor,
        nop,
        label
    }
    
    class fake_asm {
        internal opcode _opcode = opcode.nop;
        internal string op1 = "";
        internal string op2 = "";
        internal string op3 = "";

        internal fake_asm(opcode _c, string p1 = "", string p2 = "", string p3 = "") {
            this._opcode = _c;
            this.op1 = p1;
            this.op2 = p2;
            this.op3 = p3;
        }

        public override string ToString() {
            if (this._opcode == opcode.label) {
                return op1 + ":";
            } else {
                StringBuilder sb_result = new StringBuilder(this._opcode.ToString("G"));
                if (this.op1 != "") {
                    sb_result.Append(" ").Append(op1);
                    if (this.op2 != "") {
                        sb_result.Append(", ").Append(op2);
                        if (this.op3 != "") {
                            sb_result.Append(", ").Append(op3);
                        }
                    }
                }
                return sb_result.ToString();
            }
        }

    }
}
