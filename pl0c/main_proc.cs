using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace pl0c {
    class main_proc {
        /// <summary>
        /// verbose
        /// </summary>
        private bool v;
        /// <summary>
        /// path to input file
        /// </summary>
        private string input_file = "";
        /// <summary>
        /// path to output file
        /// </summary>
        private string output_file = "";
        /// <summary>
        /// path to intermediate language file
        /// </summary>
        private string inter_lang_file = "";
        //
        private bool trace_switch = false;
        //
        private bool optimize_switch = false;
        //
        private bool yes_to_all = false;
        //
        private bool show_result = false;
        //
        private bool keep_asm = false;
        //
        private bool very_verbose = false;
        /// <summary>
        /// initialize
        /// </summary>
        /// <param name="f_input"></param>
        /// <param name="f_output"></param>
        /// <param name="f_interl"></param>
        /// <param name="s_trace"></param>
        /// <param name="s_optimize"></param>
        /// <param name="s_verbose"></param>
        /// <param name="s_yes"></param>
        /// <param name="s_result"></param>
        internal main_proc(string f_input, string f_output, string f_interl, bool s_trace, bool s_optimize, bool s_verbose, bool s_yes, bool s_result, bool s_keep_asm, bool s_v_v) {
            input_file = f_input;
            output_file = f_output;
            inter_lang_file = f_interl;
            trace_switch = s_trace;
            optimize_switch = s_optimize;
            v = s_verbose;
            yes_to_all = s_yes;
            show_result = s_result;
            keep_asm = s_keep_asm;
            very_verbose = s_v_v;

            //confirm info
            if (v) Console.Write("input: " + input_file +
                                 (output_file == "" ? "" : ("\noutput: " + output_file)) +
                                 (inter_lang_file == "" ? "" : ("\nintermediate language file: " + inter_lang_file)) +
                                 "\noptions: " + (trace_switch ? "trace " : "") + 
                                                 (optimize_switch ? "optimize " : "") + 
                                                 (yes_to_all ? "yes-to-all " : "") + 
                                                 ((trace_switch || optimize_switch || yes_to_all) ? "" : "none")
                                 +"\n\n");
            //strat!
            proc();
        }
        private void proc() {
            string[] f_source = new string[]{""};
            try {
                f_source = File.ReadAllLines(input_file);
            } catch (Exception ex) {
                error.error_process(error_level.fatal_error, ex.Message);
            }
            // line id
            int line_id = 0;
            int column_id = 0;
            
            
            Stack<symbol> symbol_stack = new Stack<symbol>();

            //1 loop
            while (f_source.Length > line_id) {
                f_source[line_id] = f_source[line_id].Trim();
                if(f_source[line_id].Length == 0){
                    line_id++;
                    continue;
                }

                /* lex analyze */
                List<symbol> lex_result = new List<symbol>();

                while (column_id < f_source[line_id].Length) {
                    symbol sym;
                    try {
                        sym = new symbol(column_id, f_source[line_id], line_id);
                        if (v) Console.WriteLine(sym.id + "-\"" + sym.name + "\"");
                        column_id += int.Parse(sym.id.Split('-')[3]);
                        if (sym.name != " " && sym.name != "\t") lex_result.Add(sym);
                    } catch (Exception ex) {
                        error.error_process(error_level.normal_error, ex.Message, false);
                        column_id += (int)ex.Data["skip-length"];
                    }
                }

                /* grammar analyze and translate */
                Queue<symbol> lex_result_queue = new Queue<symbol>(lex_result);

                while (lex_result_queue.Count > 0) {
                    symbol_stack.Push(lex_result_queue.Dequeue());
                    //if (symbol_stack.Peek().name != ";" && lex_result_queue.Count > 0) continue;
                    symbol next = lex_result_queue.Count == 0 ? null : lex_result_queue.Peek();
                    try {
                        grammar_analyze_result result = grammar_analyze.analyze_and_translate(symbol_stack, next);
                        while (result.success == true) {
                            while (result.reduction_length > 0) {
                                result.reduction_length--;
                                symbol_stack.Pop();
                            }
                            if (result.reduction_result != null) symbol_stack.Push(result.reduction_result);
                            result = grammar_analyze.analyze_and_translate(symbol_stack, next);
                        }
                    } catch (Exception ex) {
                        if (ex.Data.Contains("type")) {
                            error.error_process((error_type)ex.Data["type"] == error_type.internal_code_error ? error_level.fatal_error : error_level.normal_error, ex.Message);
                        } else {
                            error.error_process(error_level.fatal_error, ex.ToString());
                        }
                    }
                }
                line_id++;
                column_id = 0;
            }
            if (error.error_has_occurred) Environment.Exit(2);
            /* final error detect */
            if (!analyze_condition.symbol_table.Exists(x => x.type == symbol_type.program_name)) {
                error.error_process(error_level.fatal_error, "Missing program name.");
            }
            List<symbol> final_symbols = symbol_stack.ToList();
            final_symbols.Reverse();
            for (int i = 0; i < final_symbols.Count - 1; i++) {
                if ((final_symbols[i].type == symbol_type.statement || final_symbols[i].type == symbol_type.variant_declaration || final_symbols[i].type == symbol_type.constant_declaration) && final_symbols[i + 1].type == symbol_type.statement) {
                    string[] info = final_symbols[i].id.Split('-');
                    error.error_process(error_level.normal_error, "Line " + info[1] + " : missing ';'.");
                }
            }
            if (analyze_condition.multi_statement_level > 0) {
                error.error_process(error_level.normal_error, "BEGIN-END pair not completed.");
            }
            if (error.error_has_occurred) Environment.Exit(2);

            if (final_symbols.Count > 2) {
                StringBuilder sb_info = new StringBuilder();
                foreach (symbol s in final_symbols) sb_info.Append(s.name).Append(" - ").AppendLine(s.id);
                error.error_process(error_level.normal_error, "unknown grammar error.(" + final_symbols.Count.ToString() + ")");
                error.error_process(error_level.information, "maybe useful: \n" + sb_info.ToString());
                Environment.Exit(2);
            }

            /* finishing translate */
            if (symbol_stack.Peek().type == symbol_type.statement) symbol_stack.Peek().control_properties.backpatch(control_property.list_type.next_list, analyze_condition.next_quaternion_line_no);
            if (v) {
                error.error_process(error_level.information,"quaternions (before optimizing) :" + analyze_condition.quaternion_list.Count.ToString());
                foreach (quaternion q in analyze_condition.quaternion_list) Console.WriteLine(q.ToString());
                error.error_process(error_level.information, "symbol table (before optimizing) :");
                foreach (symbol s in analyze_condition.symbol_table) Console.WriteLine("name: " + s.name.PadRight(10, ' ') + "\tvalue: " + s.value.ToString());
            }

            string variant_result = "";

            if (very_verbose) error.error_process(error_level.information, "calculating --->");

            /* show variant result */
            if (show_result == true) {
                try {
                    variant_result = calc_variant_result.calc(very_verbose);
                } catch (Exception ex) {
                    error.error_process(error_level.fatal_error, "Runtime Error: " + ex.ToString());
                }
            }

            /* output to intermediate language file */
            if (inter_lang_file != "") {
                try {
                    List<string> output = new List<string>() { "Program: " + analyze_condition.symbol_table.Find(x => x.type == symbol_type.program_name).name };
                    foreach (quaternion q in analyze_condition.quaternion_list) {
                        output.Add(q.ToString());
                    }
                    if (File.Exists(inter_lang_file) && yes_to_all == false) {
                        error.error_process(error_level.warning, inter_lang_file + " exists, overwrite? (y/n)");
                        string str_in = Console.ReadLine();
                        if (str_in.ToUpper() != "Y") {
                            error.error_process(error_level.fatal_error, "user cancelled.");
                        }
                    }

                    File.WriteAllLines(inter_lang_file, output.ToArray(), Encoding.ASCII);

                } catch (Exception ex) {
                    error.error_process(error_level.fatal_error, ex.Message);
                }
            }

            /* output to asm file and assemble it */
            if (output_file != "") {
                try {
                    quaternion_to_asm.convert(optimize_switch, keep_asm, trace_switch, output_file, yes_to_all, v);
                } catch (Exception ex) {
                    error.error_process(error_level.fatal_error, ex.ToString());
                }
            }

            error.error_process(error_level.information, "Compilation succeeded.");

            if (show_result == true && !very_verbose) {
                Console.WriteLine();
                error.error_process(error_level.information, "Runtime calculation result --->");
                Console.WriteLine(variant_result);
            }



        }
    }
}
