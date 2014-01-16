using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace pl0c {
    class quaternion_to_asm {
        private static List<string[]> register = new List<string[]>() {
                new string[]{"eax",""},
                new string[]{"ebx",""},
                new string[]{"ecx",""},
                new string[]{"edx",""},
            };
        private static StringBuilder sb_asm = new StringBuilder();
        private static readonly string s_constant = "Constant";
        private static readonly string s_false = "False";

        internal static List<fake_asm> fake_asm_list = new List<fake_asm>();

        internal static void convert(bool optimize_switch, bool keep_asm, bool trace_switch,string filename,bool yes_to_all = false,bool v = false) {
            
            sb_asm = new StringBuilder();

            #region translate to asm
            /* TODO :header */
            sb_asm.AppendLine("format PE console");
            sb_asm.AppendLine("entry start\n");
            sb_asm.AppendLine("include 'INCLUDE\\WIN32A-MOD.INC'\n");
            sb_asm.AppendLine("section '.text' code readable executable\n");
            sb_asm.AppendLine("\nstart:\n");
            
            /* change variant name for the sake of collision */
            for (int i = 1 ; i < analyze_condition.symbol_table.Count; i++) {
                string name = analyze_condition.symbol_table[i].name;
                if (name.StartsWith("_t_")) continue;
                if (name.StartsWith("_i_")) continue;
                for (int j = 0; j < analyze_condition.quaternion_list.Count; j++) {
                    if (analyze_condition.quaternion_list[j].left == name) analyze_condition.quaternion_list[j].left = name + "_";
                    if (analyze_condition.quaternion_list[j].right == name) analyze_condition.quaternion_list[j].right = name + "_";
                    if (analyze_condition.quaternion_list[j].result == name) analyze_condition.quaternion_list[j].result = name + "_";
                }
                analyze_condition.symbol_table[i].name = name + "_";
            }
            

            /* converting */
            convert_to_fake_asm();
            /* optimizing */
            if (optimize_switch) optimizer.begin();
            /* write to asm */
            foreach (fake_asm f in fake_asm_list) sb_asm.AppendLine(f.ToString());

            sb_asm.AppendLine("exit:");

            /* trace */
            if (trace_switch) {
                      /* HANDLE WINAPI GetProcessHeap(void); */
                sb_asm.AppendLine("invoke  GetProcessHeap")
                      .AppendLine("mov     [_hheap],eax")
                    /* LPVOID WINAPI HeapAlloc(
                     *    __in  HANDLE hHeap,
                     *    __in  DWORD dwFlags,
                     *    __in  SIZE_T dwBytes
                     * );                                         */
                      .AppendLine("invoke  HeapAlloc,[_hheap],HEAP_ZERO_MEMORY,1000h")
                      .AppendLine("mov     [_strbuf],eax")
                    /* int __cdecl wsprintf(
                     *   __out  LPTSTR lpOut,
                     *   __in   LPCTSTR lpFmt,
                     *   __in    ...
                     *   );                                       */
                      .AppendLine("cinvoke wsprintf,[_strbuf],_fmt_title,_prog_name")
                    /* HANDLE WINAPI GetStdHandle(
                     *     __in  DWORD nStdHandle
                     * );                             
                     * STD_OUTPUT_HANDLE (DWORD)-11               */
                      .AppendLine("invoke  GetStdHandle,STD_OUTPUT_HANDLE")
                      .AppendLine("mov     [_std_handle],eax")
                    /* int WINAPI lstrlen(
                     *    __in  LPCTSTR lpString
                     * );                                         */
                      .AppendLine("invoke  lstrlen,[_strbuf]")
                    /* BOOL WINAPI WriteConsole(
                     *    __in        HANDLE hConsoleOutput,
                     *    __in        const VOID *lpBuffer,
                     *    __in        DWORD nNumberOfCharsToWrite,
                     *    __out       LPDWORD lpNumberOfCharsWritten,
                     *    __reserved  LPVOID lpReserved
                     * );                                         */
                      .AppendLine("invoke  WriteConsole,[_std_handle],[_strbuf],eax,[_out_count],0")
                    /* PVOID RtlZeroMemory(
                     *    __in  PVOID ptr,
                     *    __in  SIZE_T cnt
                     * );                                         */
                      .AppendLine("invoke   RtlZeroMemory,[_strbuf],1000h");

                /* trace variant */
                foreach (symbol s in analyze_condition.symbol_table) {
                    if (s.name.StartsWith("_t_") || s.name.StartsWith("_i_") || s.type == symbol_type.program_name || s.type == symbol_type.constant) {
                        continue;
                    } else {
                        sb_asm.AppendLine("lea  ebx,[_" + s.name + "]")
                              .AppendLine("mov  edx,[" + s.name + "]")
                              .AppendLine("call console_write_line\n");
                    }
                }

                sb_asm  /* BOOL WINAPI HeapFree(
                         *    __in  HANDLE hHeap,
                         *    __in  DWORD dwFlags,
                         *    __in  LPVOID lpMem
                         * );                                      */
                        .AppendLine("invoke HeapFree,[_hheap],0,[_strbuf]")
                        .AppendLine("invoke ExitProcess,0");

                sb_asm.AppendLine("console_write_line: ")
                          .AppendLine("     cinvoke wsprintf,[_strbuf],_fmt_table,ebx,edx")
                          .AppendLine("     invoke  lstrlen,[_strbuf]")
                          .AppendLine("     invoke  WriteConsole,[_std_handle],[_strbuf],eax,[_out_count],0")
                          .AppendLine("     invoke  RtlZeroMemory,[_strbuf],1000h")
                          .AppendLine("     ret");
            } else {
                sb_asm.AppendLine("invoke	ExitProcess,0");
            }
            sb_asm.AppendLine("section '.data' data readable writeable\n");

            sb_asm.Append("_prog_name db '").Append(analyze_condition.symbol_table[0].name).AppendLine("',0");

            for (int i = 1; i < analyze_condition.symbol_table.Count; i++) {
                if (analyze_condition.symbol_table[i].name.StartsWith("_t_") || analyze_condition.symbol_table[i].type == symbol_type.variant) analyze_condition.symbol_table[i].value = 0;
                sb_asm.Append(analyze_condition.symbol_table[i].name).Append(" dd ").AppendLine(analyze_condition.symbol_table[i].value.ToString());
            }

            if (trace_switch) {
                foreach (symbol s in analyze_condition.symbol_table) {
                    if (s.name.StartsWith("_t_") || s.name.StartsWith("_i_") || s.type == symbol_type.program_name || s.type == symbol_type.constant) {
                        continue;
                    } else {
                        sb_asm.Append("_").Append(s.name).Append("\t").Append("db '").Append(s.name.Substring(0, s.name.Length - 1)).AppendLine("',0");
                    }
                }

                sb_asm.AppendLine("_strbuf      dd ?")
                      .AppendLine("_std_handle  dd 0")
                      .AppendLine("_out_count   dd 0")
                      .AppendLine("_hheap       dd 0")
                      .AppendLine("_fmt_title   db 'Program: %s',13,10,0")
                      .AppendLine("_fmt_table   db 'variant name: %s \tvalue: %d',13,10,0");
            }


            sb_asm.AppendLine("section '.idata' import data readable\n\n");

            if (trace_switch) {
                sb_asm.AppendLine(@"library kernel,'KERNEL32.DLL',\")
                      .AppendLine("user,'USER32.DLL'")
                      .AppendLine()
                      .AppendLine(@"import kernel,\")
                      .AppendLine(@"    GetProcessHeap,'GetProcessHeap',\")
                      .AppendLine(@"    HeapAlloc,'HeapAlloc',\")
                      .AppendLine(@"    HeapFree,'HeapFree',\")
                      .AppendLine(@"    GetStdHandle,'GetStdHandle',\")
                      .AppendLine(@"    RtlZeroMemory,'RtlZeroMemory',\")
                      .AppendLine(@"    lstrlen,'lstrlenA',\")
                      .AppendLine(@"    WriteConsole,'WriteConsoleA',\")
                      .AppendLine(@"    ExitProcess,'ExitProcess'")
                      .AppendLine(@"    ")
                      .AppendLine()
                      .AppendLine(@"import user,\")
                      .AppendLine(@"    wsprintf,'wsprintfA'");
            } else {
                sb_asm.AppendLine("library kernel32,'KERNEL32.DLL'");

                sb_asm.AppendLine("import kernel32,\\").AppendLine("ExitProcess,'ExitProcess'");
            }
#endregion

            if (v) {
                error.error_process(error_level.information, "ASM File--->");
                Console.WriteLine(sb_asm.ToString());
            }

            string path_to_asm_file = Path.GetFullPath(Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + DateTime.Now.ToString("-yyyy-MM-dd-hh-mm-ss-ffff") + ".asm");
            
            File.WriteAllText(path_to_asm_file, sb_asm.ToString());

            if (File.Exists(filename) && yes_to_all == false) {
                error.error_process(error_level.warning, filename + " exists, overwrite? (y/n)");
                string str_in = Console.ReadLine();
                if (str_in.ToUpper() != "Y") {
                    error.error_process(error_level.fatal_error, "user cancelled.");
                }
            }

            StringBuilder sb_output = new StringBuilder();

            Process p = new Process();
            
            p.StartInfo.FileName = "FASM.EXE";
            p.StartInfo.Arguments = path_to_asm_file + " " + filename;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;

            p.Start();

            p.StandardOutput.ReadLine();
            sb_output.Append(p.StandardOutput.ReadToEnd());

            p.WaitForExit();

            if (v) {
                error.error_process(error_level.information, "FASM output --->");
                Console.WriteLine(sb_output.ToString());
            }
            if (p.ExitCode != 0) {
                // failed
                error.error_process(error_level.fatal_error, "error occurred while assembling, internal error.");
            }

            if (keep_asm) {
                error.error_process(error_level.information, "ASM file has been saved to " + path_to_asm_file);
            } else {
                File.Delete(path_to_asm_file);
            }

            
        }

        internal static void convert_to_fake_asm() {
            /* make label */
            int label_no = 0;
            Dictionary<int, string> label = new Dictionary<int, string>();
            foreach (quaternion q in analyze_condition.quaternion_list) {
                if (q.next != -1 && q.next != analyze_condition.next_quaternion_line_no) {
                    try {
                        label.Add(q.next, "L" + label_no.ToString("D3"));
                        label_no += 1;
                    } catch {
                        continue;
                    }
                }
            }
            label.Add(analyze_condition.next_quaternion_line_no, "exit");


            /* convert_to_fake_asm */
            fake_asm_list = new List<fake_asm>();

            int i_left = 0, i_right = 0;
            string s_left = "", s_right = "", s_result = "";

            foreach (quaternion cur in analyze_condition.quaternion_list) {
                if (cur.left != "") i_left = analyze_condition.symbol_table.FindIndex(x => x.name == cur.left);
                if (cur.right != "") i_right = analyze_condition.symbol_table.FindIndex(x => x.name == cur.right);
                if (cur.result != "") i_left = analyze_condition.symbol_table.FindIndex(x => x.name == cur.result);
                if (label.ContainsKey(cur.id)) {
                    fake_asm_list.Add(new fake_asm(opcode.label, label[cur.id]));
                }
                if (cur.action == quaternion_action.jmp) {
                    fake_asm_list.Add(new fake_asm(opcode.jmp, label[cur.next]));
                } else if (cur.action < quaternion_action.mov && cur.action != quaternion_action.jmp) {
                    s_left = get_reg(cur.left);
                    s_right = get_reg_or_imm(cur.right);
                    fake_asm_list.Add(new fake_asm(opcode.cmp, s_left, s_right));
                    fake_asm_list.Add(new fake_asm((opcode)cur.action, label[cur.next]));
                    clear_reg_link(s_left);
                    clear_reg_link(s_right);
                } else if (cur.action == quaternion_action.mov) {
                    s_right = get_reg_or_imm(cur.right);
                    fake_asm_list.Add(new fake_asm(opcode.mov, "[" + cur.left + "]", s_right));
                    clear_reg_link(s_right);
                } else if (cur.action == quaternion_action.add || cur.action == quaternion_action.sub) {
                    s_left = get_reg(cur.left);
                    if (get_reg_or_imm(cur.right, true) == s_false) {
                        fake_asm_list.Add(new fake_asm((opcode)cur.action, s_left, "[" + cur.right + "]"));
                    } else {
                        fake_asm_list.Add(new fake_asm((opcode)cur.action, s_left, get_reg_or_imm(cur.right)));
                    }
                    if (get_reg_or_imm(cur.result, true) == s_false) {
                        fake_asm_list.Add(new fake_asm(opcode.mov, "[" + cur.result + "]", s_left));
                    } else {
                        fake_asm_list.Add(new fake_asm(opcode.mov, get_reg(cur.result), s_left));
                    }
                    clear_reg_link(s_left);
                } else if (cur.action == quaternion_action.mul) {
                    s_left = get_reg_or_imm(cur.left, true);
                    s_right = get_reg_or_imm(cur.right, true);
                    s_result = get_reg_or_imm(cur.result);
                    if (s_left == s_constant && s_right != s_constant) {
                        string temp = s_left; s_left = s_right; s_right = temp;
                        temp = cur.left; cur.left = cur.right; cur.right = temp;
                    }
                    if (s_left == s_constant) {
                        s_left = register[0][0];
                        clear_set_imm_spec_reg(register[0][0], true, analyze_condition.symbol_table.Find(x => x.name == cur.left).value);
                    }
                    if (s_left == s_result) {
                        // imul reg32,mem32/reg32
                        string op1 = s_result;
                        string op2 = "";
                        if (s_right == s_false) {
                            op2 = "[" + cur.right + "]";
                        } else if (s_right == s_constant) {
                            op2 = analyze_condition.symbol_table.Find(x => x.name == cur.right).value.ToString();
                        } else {
                            op2 = s_right;
                        }
                        fake_asm_list.Add(new fake_asm(opcode.imul, op1, op2));
                    } else {
                        if (s_right != s_constant) {
                            s_right = get_reg(cur.right);
                            if (s_left == s_false) {
                                fake_asm_list.Add(new fake_asm(opcode.imul, s_right, "[" + cur.left + "]"));
                                fake_asm_list.Add(new fake_asm(opcode.mov, s_result, s_right));
                                clear_reg_link(s_right);
                            } else {
                                fake_asm_list.Add(new fake_asm(opcode.imul, s_left, s_right));
                                fake_asm_list.Add(new fake_asm(opcode.mov, s_result, s_left));
                                clear_reg_link(s_left);
                            }
                        } else {
                            // imul reg32,mem32/reg32,imm32
                            fake_asm_list.Add(new fake_asm(opcode.imul, s_result, s_left == s_false ? ("[" + cur.left + "]") : s_left, analyze_condition.symbol_table.Find(x => x.name == cur.right).value.ToString()));
                        }
                    }
                } else if (cur.action == quaternion_action.div) {
                    /* clear eax,edx
                     * mov left,eax
                     * cdq
                     * idiv right
                     * mov result,eax
                     * */
                    s_left = get_reg_or_imm(cur.left, true);
                    if (s_left == s_constant) {
                        clear_set_imm_spec_reg("eax", true, analyze_condition.symbol_table.Find(x => x.name == cur.right).value);
                    } else if (s_left == s_false) {
                        clear_set_imm_spec_reg("eax");
                        set_spec_reg("eax", cur.left);
                    } else {
                        exchange_reg("eax", s_left);
                    }
                    clear_set_imm_spec_reg("edx");
                    fake_asm_list.Add(new fake_asm(opcode.cdq));
                    s_right = get_reg_or_imm(cur.right, true);
                    if (s_right != s_false && s_right != s_constant) {
                        clear_set_imm_spec_reg(s_right);
                    }
                    fake_asm_list.Add(new fake_asm(opcode.idiv, "[" + cur.right + "]"));
                    s_result = get_reg(cur.result);
                    fake_asm_list.Add(new fake_asm(opcode.mov, s_result, "eax"));
                    clear_reg_link("eax");
                } else if (cur.action == quaternion_action.neg) {
                    s_left = get_reg_or_imm(cur.left,true);
                    if (s_left != s_constant) {
                        s_left = get_reg(cur.left);
                        fake_asm_list.Add(new fake_asm(opcode.neg, s_left));
                        s_right = get_reg(cur.right, true);
                        if (s_right == s_false) {
                            set_reg_link(s_left, cur.right);
                        } else {
                            fake_asm_list.Add(new fake_asm(opcode.mov, "[" + cur.right + "]", s_left));
                        }
                        
                    }
                }
            }

        }

        private static string get_reg(string var_name, bool just_find_it_out = false) {
            int var_name_id = register.FindIndex(x => x[1] == var_name);
            string reg_result = "";
            if (var_name_id != -1) {
                reg_result = register[var_name_id][0];
                if (var_name_id != 3) {
                    register.RemoveAt(var_name_id);
                    register.Add(new string[] { reg_result, var_name });
                }
                return reg_result;
            } else {
                if (just_find_it_out == true) return s_false;
                reg_result = register[0][0];
                if (register[0][1] != "" && register[0][1] != null)
                    fake_asm_list.Add(new fake_asm(opcode.mov, "[" + register[0][1] + "]", reg_result));
                register.RemoveAt(0);
                register.Add(new string[] { reg_result, var_name });
                if (get_reg_or_imm(var_name, true) == s_constant) {
                    fake_asm_list.Add(new fake_asm(opcode.mov, reg_result, analyze_condition.symbol_table.Find(x => x.name == var_name).value.ToString()));
                } else {
                    fake_asm_list.Add(new fake_asm(opcode.mov, reg_result, "[" + var_name + "]"));
                }
                return reg_result;
            }
        }

        private static string get_reg_or_imm(string var_name, bool just_find_it_out = false) {
            int var_in_table = analyze_condition.symbol_table.FindIndex(x => (x.type == symbol_type.constant || x.type == symbol_type.integer) && x.name == var_name);
            if (var_in_table != -1) {
                if (just_find_it_out) return s_constant;
                return analyze_condition.symbol_table[var_in_table].value.ToString();
            }
            return get_reg(var_name, just_find_it_out);
        }

        private static void clear_set_imm_spec_reg(string reg_name,bool set_imm = false,int imm = 0) {
            int reg_id = register.FindIndex(x => x[0] == reg_name);
            if (register[reg_id][1] != "") fake_asm_list.Add(new fake_asm(opcode.mov, "[" + register[reg_id][1] + "]", reg_name));
            if (set_imm) {
                fake_asm_list.Add(new fake_asm(opcode.mov, reg_name, imm.ToString()));
            } else {
                fake_asm_list.Add(new fake_asm(opcode.xor, reg_name, reg_name));
            }
            if (reg_id != 3) {
                register.RemoveAt(reg_id);
                register.Add(new string[] { reg_name, "" });
            }
        }

        private static void set_spec_reg(string reg_name, string var_name) {
            int reg_id = register.FindIndex(x => x[0] == reg_name);
            fake_asm_list.Add(new fake_asm(opcode.mov, reg_name, "[" + var_name + "]"));
            if (reg_id != 3) {
                register.RemoveAt(reg_id);
                register.Add(new string[] { reg_name, var_name });
            }
        }

        private static void exchange_reg(string reg_a, string reg_b) {
            if (reg_a == reg_b) {
                return;
            } else {
                int reg_a_i = register.FindIndex(x => x[0] == reg_a);
                int reg_b_i = register.FindIndex(x => x[0] == reg_b);
                register[reg_a_i][0] = reg_b;
                register[reg_b_i][0] = reg_a;
                fake_asm_list.Add(new fake_asm(opcode.xchg, reg_a, reg_b));
            }
        }

        private static void clear_reg_link(string reg_name) {
            int reg_id = register.FindIndex(x => x[0] == reg_name);
            if (reg_id == -1) return;
            register.RemoveAt(reg_id);
            register.Insert(0, new string[] { reg_name, "" });
        }

        private static void set_reg_link(string reg_name, string var_name) {
            int reg_id = register.FindIndex(x => x[0] == reg_name);
            if (reg_id == -1) return;
            register.RemoveAt(reg_id);
            register.Insert(0, new string[] { reg_name, var_name });
        }

    }
}
