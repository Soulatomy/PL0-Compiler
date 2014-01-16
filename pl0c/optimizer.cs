using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pl0c {
    class optimizer {
        internal static void begin() {
            delete_constant_reference();

            delete_cross_assignment();

            add_sub_1_to_inc_dec();

            delete_same_register_assignment();

            delete_double_jump();

            delete_no_use_temp_variant();

            delete_no_use_label();
        }

        private static void delete_constant_reference() {
            for (int i = 0; i < quaternion_to_asm.fake_asm_list.Count; i++) {
                if (quaternion_to_asm.fake_asm_list[i].op1.StartsWith("[_i_") && quaternion_to_asm.fake_asm_list[i]._opcode != opcode.idiv) {
                    string const_name = quaternion_to_asm.fake_asm_list[i].op1.Substring(1, quaternion_to_asm.fake_asm_list[i].op1.Length - 2);
                    quaternion_to_asm.fake_asm_list[i].op1 = analyze_condition.symbol_table.Find(x => x.name == const_name).value.ToString();
                }
            }
        }

        private static void delete_cross_assignment() {
            /* **
             * like:
             * mov eax,[_t_001]
             * mov [_t_001],eax
             * change to: 
             * mov eax,[_t_001]
             * */
            int i = 1;
            while (i < quaternion_to_asm.fake_asm_list.Count) {
                fake_asm prev_asm = quaternion_to_asm.fake_asm_list[i - 1];
                fake_asm curr_asm = quaternion_to_asm.fake_asm_list[i];
                if (prev_asm._opcode == opcode.mov && curr_asm._opcode == opcode.mov) {
                    if (prev_asm.op1 == curr_asm.op2 && prev_asm.op2 == curr_asm.op1) {
                        quaternion_to_asm.fake_asm_list.RemoveAt(i);
                    }
                }
                i += 1;
            }
        }

        private static void delete_no_use_temp_variant() {
            foreach (fake_asm f in quaternion_to_asm.fake_asm_list) {
                if (f.op1 != "" && f.op1.StartsWith("[")) analyze_condition.symbol_table[analyze_condition.symbol_table.FindIndex(x => x.name == f.op1.Substring(1, f.op1.Length - 2))].hit_count++;
                if (f.op2 != "" && f.op2.StartsWith("[")) analyze_condition.symbol_table[analyze_condition.symbol_table.FindIndex(x => x.name == f.op2.Substring(1, f.op2.Length - 2))].hit_count++;
                if (f.op3 != "" && f.op3.StartsWith("[")) analyze_condition.symbol_table[analyze_condition.symbol_table.FindIndex(x => x.name == f.op3.Substring(1, f.op3.Length - 2))].hit_count++;
            }
            analyze_condition.symbol_table.RemoveAll(x => x.type != symbol_type.program_name && x.hit_count == 0);
        }

        private static void delete_double_jump() {
            /* **
             * like:
             *      jcc L1
             *      jmp L2
             * L1:  
             * ...
             * L2:
             * 
             * change to:
             *      jncc L2
             * ...
             * L2:
             * */
            int i = 0;
            while (i < quaternion_to_asm.fake_asm_list.Count - 2) {
                if (quaternion_to_asm.fake_asm_list[i]._opcode < opcode.mov && quaternion_to_asm.fake_asm_list[i]._opcode > opcode.jmp) {
                    if (quaternion_to_asm.fake_asm_list[i + 1]._opcode == opcode.jmp) {
                        if (quaternion_to_asm.fake_asm_list[i + 2]._opcode == opcode.label) {
                            if (quaternion_to_asm.fake_asm_list[i + 2].op1 == quaternion_to_asm.fake_asm_list[i].op1) {
                                switch (quaternion_to_asm.fake_asm_list[i]._opcode) {
                                    case opcode.je:
                                        quaternion_to_asm.fake_asm_list[i]._opcode = opcode.jne;
                                        break;
                                    case opcode.jne:
                                        quaternion_to_asm.fake_asm_list[i]._opcode = opcode.je;
                                        break;
                                    case opcode.jg:
                                        quaternion_to_asm.fake_asm_list[i]._opcode = opcode.jle;
                                        break;
                                    case opcode.jle:
                                        quaternion_to_asm.fake_asm_list[i]._opcode = opcode.jg;
                                        break;
                                    case opcode.jge:
                                        quaternion_to_asm.fake_asm_list[i]._opcode = opcode.jl;
                                        break;
                                    case opcode.jl:
                                        quaternion_to_asm.fake_asm_list[i]._opcode = opcode.jge;
                                        break;                                        
                                }
                                quaternion_to_asm.fake_asm_list[i].op1 = quaternion_to_asm.fake_asm_list[i + 1].op1;
                                quaternion_to_asm.fake_asm_list.RemoveAt(i + 1);
                                quaternion_to_asm.fake_asm_list.RemoveAt(i + 1);
                            }
                        }
                    }
                }
                i += 1;
            }
        }

        private static void delete_no_use_label() {
            var temp = from asm in quaternion_to_asm.fake_asm_list
                       where asm._opcode == opcode.label
                       select asm.op1;
            List<string> labels = new List<string>(temp);
            foreach (fake_asm asm in quaternion_to_asm.fake_asm_list) {
                if (asm._opcode < opcode.mov && labels.Contains(asm.op1)) labels.Remove(asm.op1);
            }
            quaternion_to_asm.fake_asm_list.RemoveAll(x => x._opcode == opcode.label && labels.Contains(x.op1));
        }

        private static void add_sub_1_to_inc_dec(){
            /* **
             * like:
             * mov ebx, [x_]
             * add ebx, 1
             * mov [_t_002], ebx
             * mov [x_], ebx
             * change to:
             * inc [x_]
             * */
            int i = 0;
            fake_asm f1, f2, f3, f4, asm;
            while (i < quaternion_to_asm.fake_asm_list.Count - 3) {
                f1 = quaternion_to_asm.fake_asm_list[i];
                f2 = quaternion_to_asm.fake_asm_list[i + 1];
                f3 = quaternion_to_asm.fake_asm_list[i + 2];
                f4 = quaternion_to_asm.fake_asm_list[i + 3];

                if (f1._opcode == opcode.mov && C.data_gpr.Contains(f1.op1) && f1.op2.StartsWith("[")) {
                    if (f2.op1 == f1.op1 && f2.op2 == "1") {
                        if (f3._opcode == opcode.mov && f3.op1.StartsWith("[") && f3.op2 == f1.op1) {
                            if (f4._opcode == opcode.mov && f4.op1 == f1.op2 && f4.op2 == f1.op1) {
                                if (f2._opcode == opcode.add || f2._opcode == opcode.sub) {
                                    asm = new fake_asm(f2._opcode == opcode.add ? opcode.inc : opcode.dec, f1.op2);
                                    quaternion_to_asm.fake_asm_list[i] = asm;
                                    quaternion_to_asm.fake_asm_list.RemoveAt(i + 1);
                                    quaternion_to_asm.fake_asm_list.RemoveAt(i + 1); 
                                    quaternion_to_asm.fake_asm_list.RemoveAt(i + 1);
                                }
                            }
                        }
                    }
                }
                i += 1;
            }
        }

        private static void delete_same_register_assignment() {
            /* *
             * like:
             * mov ebx, [_t_012]
             * mov ebx, eax
             * 
             * change to:
             * mov ebx,eax
             * */
            int i = 0;
            fake_asm f1, f2;
            while (i < quaternion_to_asm.fake_asm_list.Count - 1) {
                f1 = quaternion_to_asm.fake_asm_list[i];
                f2 = quaternion_to_asm.fake_asm_list[i + 1];
                if (f1._opcode == opcode.mov && f2._opcode == opcode.mov && f1.op1 == f2.op1) {
                    quaternion_to_asm.fake_asm_list.RemoveAt(i);
                }
                i += 1;
            }
        }
    }
}
