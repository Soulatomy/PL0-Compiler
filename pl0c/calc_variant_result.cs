using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pl0c {
    class calc_variant_result {
        internal static string calc (bool v = false) {
            List<symbol> runtime_symbol_table = new List<symbol>(analyze_condition.symbol_table);
            int eip = 0;
            int i_left = 0, i_right = 0, i_result = 0;
            #region processing
            while (eip >= 0 && eip < analyze_condition.quaternion_list.Count) {
                quaternion cur = analyze_condition.quaternion_list[eip];
                if (v) {
                    error.error_process(error_level.information, cur.ToString());
                    foreach (symbol s in runtime_symbol_table) {
                        Console.WriteLine("name: " + s.name.PadRight(10) + "\tvalue :" + s.value.ToString());
                    }
                }


                switch (cur.action) {
                    case quaternion_action.jmp:
                        eip = cur.next;
                        break;
                    case quaternion_action.je:
                        eip = runtime_symbol_table.Find(x => x.name == cur.left).value == runtime_symbol_table.Find(x => x.name == cur.right).value ? cur.next : eip + 1;
                        break;
                    case quaternion_action.jne:
                        eip = runtime_symbol_table.Find(x => x.name == cur.left).value != runtime_symbol_table.Find(x => x.name == cur.right).value ? cur.next : eip + 1;
                        break;
                    case quaternion_action.jg:
                        eip = runtime_symbol_table.Find(x => x.name == cur.left).value > runtime_symbol_table.Find(x => x.name == cur.right).value ? cur.next : eip + 1;
                        break;
                    case quaternion_action.jge:
                        eip = runtime_symbol_table.Find(x => x.name == cur.left).value >= runtime_symbol_table.Find(x => x.name == cur.right).value ? cur.next : eip + 1;
                        break;
                    case quaternion_action.jl:
                        eip = runtime_symbol_table.Find(x => x.name == cur.left).value < runtime_symbol_table.Find(x => x.name == cur.right).value ? cur.next : eip + 1;
                        break;
                    case quaternion_action.jle:
                        eip = runtime_symbol_table.Find(x => x.name == cur.left).value <= runtime_symbol_table.Find(x => x.name == cur.right).value ? cur.next : eip + 1;
                        break;
                    case quaternion_action.mov:
                        i_left = runtime_symbol_table.FindIndex(x => x.name == cur.left);
                        i_right = runtime_symbol_table.FindIndex(x => x.name == cur.right);
                        runtime_symbol_table[i_left].value = runtime_symbol_table[i_right].value;
                        eip++;
                        break;
                    case quaternion_action.neg:
                        i_left = runtime_symbol_table.FindIndex(x => x.name == cur.left);
                        i_right = runtime_symbol_table.FindIndex(x => x.name == cur.right);
                        i_result = runtime_symbol_table.FindIndex(x => x.name == cur.result);
                        runtime_symbol_table[i_right].value = -runtime_symbol_table[i_left].value;
                        eip++;
                        break;
                    case quaternion_action.add:
                        i_left = runtime_symbol_table.FindIndex(x => x.name == cur.left);
                        i_right = runtime_symbol_table.FindIndex(x => x.name == cur.right);
                        i_result = runtime_symbol_table.FindIndex(x => x.name == cur.result);
                        runtime_symbol_table[i_result].value = runtime_symbol_table[i_left].value + runtime_symbol_table[i_right].value;
                        eip++;
                        break;
                    case quaternion_action.sub:
                        i_left = runtime_symbol_table.FindIndex(x => x.name == cur.left);
                        i_right = runtime_symbol_table.FindIndex(x => x.name == cur.right);
                        i_result = runtime_symbol_table.FindIndex(x => x.name == cur.result);
                        runtime_symbol_table[i_result].value = runtime_symbol_table[i_left].value - runtime_symbol_table[i_right].value; 
                        eip++;
                        break;
                    case quaternion_action.mul:
                        i_left = runtime_symbol_table.FindIndex(x => x.name == cur.left);
                        i_right = runtime_symbol_table.FindIndex(x => x.name == cur.right);
                        i_result = runtime_symbol_table.FindIndex(x => x.name == cur.result);
                        runtime_symbol_table[i_result].value = runtime_symbol_table[i_left].value * runtime_symbol_table[i_right].value; 
                        eip++;
                        break;
                    case quaternion_action.div:
                        i_left = runtime_symbol_table.FindIndex(x => x.name == cur.left);
                        i_right = runtime_symbol_table.FindIndex(x => x.name == cur.right);
                        i_result = runtime_symbol_table.FindIndex(x => x.name == cur.result);
                        runtime_symbol_table[i_result].value = runtime_symbol_table[i_left].value / runtime_symbol_table[i_right].value; 
                        eip++;
                        break;
                }
                
            }
            #endregion

            StringBuilder sb_result = new StringBuilder();

            sb_result.AppendLine("Program: " + runtime_symbol_table.Find(x => x.type == symbol_type.program_name).name);

            if (v) error.error_process(error_level.information, "FIN");

            foreach (symbol s in runtime_symbol_table) {
                if (v) Console.WriteLine("name: " + s.name.PadRight(10) + "\tvalue :" + s.value.ToString());
                if (s.name.StartsWith("_t_") || s.name.StartsWith("_i_") || s.type == symbol_type.program_name) {
                    continue;
                } else {
                    if (s.type == symbol_type.variant) sb_result.AppendLine("name: " + s.name.PadRight(10) + "\tvalue :" + s.value.ToString());
                }
            }

            return sb_result.ToString();
        }
    }
}
