using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pl0c {
    
    class grammar_analyze {
        internal static grammar_analyze_result analyze_and_translate(Stack<symbol> symbol_stack_in, symbol next_symbol = null) {
            grammar_analyze_result gar = new grammar_analyze_result();
            gar.reduction_result = new symbol(symbol_type.others);
            gar.success = false;

            List<symbol> symbol_list = symbol_stack_in.ToList();
            symbol_list.Reverse();

            int k = symbol_list.Count;
            //for (int k = symbol_list.Count; k > 0; k--) {
                for (int i = 0; i < k; i++) {
                    StringBuilder sb_symbols = new StringBuilder();
                    for (int j = i; j < k; j++) {
                        sb_symbols.Append(symbol_list[j].type.ToString("G") + "-");
                    }
                    string candidate_symbols = sb_symbols.ToString(0, sb_symbols.Length - 1);

                    #region reserved-identifier
                    if (candidate_symbols == "reserved-identifier") {
                        gar.reduction_length = 2;
                        if (new List<string>() { "PROGRAM", "VAR", "CONST" }.Contains(symbol_list[i].name)) {
                            if (analyze_condition.has_multi_statement_field == true) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): \"" + symbol_list[i].name + "\" should not be in the statement field.");
                                ex.Data["type"] = error_type.grammar_error;
                                throw ex;
                            }
                        }
                        if (symbol_list[i].name == "PROGRAM") {
                            if (analyze_condition.has_variant_def == true) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): no more program name declaration.");
                                ex.Data["type"] = error_type.no_more_declaration;
                                throw ex;
                            } else {
                                analyze_condition.has_program_header = true;
                            }
                            gar.reduction_result.type = symbol_type.program_definition;
                            gar.reduction_result.id = symbol_list[i + 1].id.Replace("identifier", "program_definition");
                            symbol_list[i + 1].type = symbol_type.program_name;
                            analyze_condition.symbol_table.Add(symbol_list[i + 1]);
                            gar.success = true;
                            return gar;
                        } else if (symbol_list[i].name == "VAR") {
                            if (analyze_condition.has_variant_def == true) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): no more variant declaration.");
                                ex.Data["type"] = error_type.no_more_declaration;
                                throw ex;
                            } else {
                                analyze_condition.has_variant_def = true;
                                analyze_condition.in_variant_def = true;
                            }
                            gar.reduction_result.type = symbol_type.variant_declaration;
                            gar.reduction_result.id = symbol_list[i + 1].id.Replace("identifier", "variant_declaration");
                            symbol_list[i + 1].type = symbol_type.variant;
                            if (!analyze_condition.symbol_table.Exists(x => x.name == symbol_list[i + 1].name)) {
                                analyze_condition.symbol_table.Add(symbol_list[i + 1]);
                            } else {
                                string[] info = symbol_list[i + 1].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): variant name \"" + symbol_list[i + 1].name + "\" has benn declared.");
                                ex.Data["type"] = error_type.variant_has_been_declared;
                                throw ex;
                            }
                            gar.success = true;
                            return gar;
                        } else if (symbol_list[i].name == "CONST") {
                            if (analyze_condition.has_constant_def == true) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): no more constant declaration.");
                                ex.Data["type"] = error_type.no_more_declaration;
                                throw ex;
                            }
                            gar.success = false;
                            analyze_condition.in_constant_def_field = true;
                            return gar;
                        }
                    }
                    #endregion
                    
                    #region program_definition-identifier
                    if (candidate_symbols == "program_definition-identifier") {
                        string[] info = symbol_list[i + 1].id.Split('-');
                        Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): unnecessary part after program definition, ignore.");
                        ex.Data["type"] = error_type.unnecessary_part_after_program_def;
                        throw ex;
                    }
                    #endregion

                    #region variant_declaration-delimiter-identifier
                    if (candidate_symbols == "variant_declaration-delimiter-identifier") {
                        gar.reduction_length = 3;
                        if (symbol_list[i + 1].name == ",") {
                            gar.reduction_result.type = symbol_type.variant_declaration;
                            gar.reduction_result.id = symbol_list[i + 2].id.Replace("identifier", "variant_declaration");
                            symbol_list[i + 2].type = symbol_type.variant;
                            if (!analyze_condition.symbol_table.Exists(x => x.name == symbol_list[i + 2].name)) {
                                analyze_condition.symbol_table.Add(symbol_list[i + 2]);
                            } else {
                                string[] info = symbol_list[i + 2].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): variant name \"" + symbol_list[i + 2].name + "\" has benn declared.");
                                ex.Data["type"] = error_type.variant_has_been_declared;
                                throw ex;
                            }
                            gar.success = true;
                            return gar;
                        } else {
                            string[] info = symbol_list[i + 1].id.Split('-');
                            Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): wrong delimiter in this place, should be \",\" .");
                            ex.Data["type"] = error_type.wrong_delimiter;
                            throw ex;
                        }
                    }
                    #endregion

                    #region reserved-constant_definition
                    if (candidate_symbols == "reserved-constant_definition") {
                        gar.reduction_length = 2;
                        if (symbol_list[i].name == "CONST") {
                            gar.reduction_result.type = symbol_type.constant_declaration;
                            gar.reduction_result.id = symbol_list[i + 1].id.Replace("constant_definition", "constant_declaration");
                            gar.success = true;
                            return gar;
                        } else {
                            string[] info = symbol_list[i + 1].id.Split('-');
                            Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): grammar error, wrong statement.");
                            ex.Data["type"] = error_type.grammar_error;
                            throw ex;
                        }
                    }
                    #endregion

                    #region constant_declaration-delimiter-constant_definition
                    if (candidate_symbols == "constant_declaration-delimiter-constant_definition") {
                        gar.reduction_length = 3;
                        if (symbol_list[i + 1].name == ",") {
                            gar.reduction_result.type = symbol_type.constant_declaration;
                            gar.reduction_result.id = symbol_list[i + 2].id.Replace("constant_definition", "constant_declaration");
                            gar.success = true;
                            return gar;
                        } else {
                            string[] info = symbol_list[i + 1].id.Split('-');
                            Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): grammar error, wrong delimiter.");
                            ex.Data["type"] = error_type.grammar_error;
                            throw ex;
                        }
                    }
                    #endregion

                    #region identifier-operator_rel-integer
                    if (candidate_symbols == "expression-operator_rel-expression") {
                        if (analyze_condition.in_constant_def_field == true) {
                            if (symbol_list[i + 1].name == "=" && is_temp_var(symbol_list[i].name) == false) {
                                symbol_list[i].type = symbol_type.identifier;
                                candidate_symbols = "identifier" + candidate_symbols.Substring(10);
                                candidate_symbols = candidate_symbols.Replace("expression", "integer");
                            }
                        }

                    }
                    if (candidate_symbols == "identifier-operator_rel-integer") {
                        if (symbol_list[i + 1].name == "=") {
                            if (analyze_condition.in_constant_def_field == true) {
                                gar.reduction_length = 3;
                                gar.reduction_result.type = symbol_type.constant_definition;
                                gar.reduction_result.id = symbol_list[i + 2].id.Replace("integer", "constant_definition");
                                if (analyze_condition.symbol_table.Exists(x => x.name == symbol_list[i].name)) {
                                    string[] info = symbol_list[i].id.Split('-');
                                    Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): constant \"" + symbol_list[i].name + "\" exists.");
                                    ex.Data["type"] = error_type.constant_exists;
                                    throw ex;
                                } else {
                                    symbol_list[i].type = symbol_type.constant;
                                    symbol_list[i].value = symbol_list[i + 2].value;
                                    analyze_condition.symbol_table.Add(symbol_list[i]);
                                    gar.success = true;
                                    return gar;
                                }
                            }
                        }
                    }
                    #endregion

                    #region CONST ... ;
                    if (candidate_symbols == "constant_declaration-delimiter") {
                        if (symbol_list[i + 1].name == ";") {
                            analyze_condition.in_constant_def_field = false;
                            analyze_condition.has_constant_def = true;
                            gar.success = true;
                            gar.reduction_length = 2;
                            gar.reduction_result = null;
                            return gar;
                        }
                    }
#endregion

                    #region VAR ... ;
                    if (candidate_symbols == "variant_declaration-delimiter") {
                        if (symbol_list[i + 1].name == ";") {
                            analyze_condition.in_variant_def = false;
                            analyze_condition.has_variant_def = true;
                            gar.success = true;
                            gar.reduction_length = 2;
                            gar.reduction_result = null;
                            return gar;
                        }
                    }
                    #endregion

                    #region BEGIN & END
                    if (candidate_symbols == "complex_statement_start") {
                        gar.success = false;
                        analyze_condition.has_multi_statement_field = true;
                        analyze_condition.multi_statement_level += 1;
                        return gar;
                    }

                    if (candidate_symbols == "complex_statement_start-statement-complex_statement_end") {
                        gar.reduction_length = 3;
                        gar.reduction_result = symbol_list[i + 1];
                        gar.success = true;
                        analyze_condition.multi_statement_level -= 1;
                        return gar;
                    }
                    if (candidate_symbols == "complex_statement_start-statement-delimiter-complex_statement_end") {
                        if (symbol_list[i + 2].name == ";") {
                            gar.reduction_length = 4;
                            gar.reduction_result = symbol_list[i + 1];
                            gar.success = true;
                            analyze_condition.multi_statement_level -= 1;
                            return gar;
                        } else {
                            string[] info = symbol_list[i + 2].id.Split('-');
                            Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): wrong delimiter, should be ';'.");
                            ex.Data["type"] = error_type.wrong_delimiter;
                            throw ex;
                        }
                        
                    }

                    if (candidate_symbols == "complex_statement_start-complex_statement_end") {
                        gar.reduction_length = 2;
                        gar.reduction_result = null;
                        gar.success = true;
                        analyze_condition.multi_statement_level -= 1;
                        return gar;
                    }
                    #endregion

                    #region identifier-delimiter-expression
                    if (candidate_symbols == "expression-delimiter-expression") {
                        if (symbol_list[i + 1].name == ":=" && is_temp_var(symbol_list[i].name) == false) {
                            symbol_list[i].type = symbol_type.identifier;
                            candidate_symbols = "identifier" + candidate_symbols.Substring(10);
                        }
                    }
                    if (candidate_symbols == "identifier-delimiter-expression") {
                        gar.reduction_length = 3;
                        int indentifier_in_symbol_table = analyze_condition.symbol_table.FindIndex(x => x.name == symbol_list[i].name);
                        if (symbol_list[i + 1].name == ":=") {
                            if (!analyze_condition.symbol_table.Exists(x => x.name == symbol_list[i + 2].name)) {
                                string[] info = symbol_list[i + 2].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): expression error.");
                                ex.Data["type"] = error_type.expression_error;
                                throw ex;
                            } else if (indentifier_in_symbol_table == -1) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): unknown symbol \"" + symbol_list[i].name + "\" .");
                                ex.Data["type"] = error_type.unknown_symbol;
                                throw ex;
                            } else {
                                if (analyze_condition.symbol_table[indentifier_in_symbol_table].type != symbol_type.variant) {
                                    string[] info = symbol_list[i + 1].id.Split('-');
                                    Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): value should be assigned to a variant.");
                                    ex.Data["type"] = error_type.constant_value_assignment;
                                    throw ex;
                                }
                                if (next_symbol != null) {
                                    if (next_symbol.type == symbol_type.operator_add_sub || next_symbol.type == symbol_type.operator_mul_div) {
                                        return gar;
                                    }
                                }
                                quaternion _q = new quaternion(quaternion_action.mov, symbol_list[i].name, symbol_list[i + 2].name);
                                analyze_condition.quaternion_list.Add(_q);
                                gar.reduction_result.type = symbol_type.assignment_statement;
                                gar.reduction_result.control_properties = new control_property(true);
                                gar.reduction_result.control_properties.next_list.Add(analyze_condition.next_quaternion_line_no);
                                gar.reduction_result.id = symbol_list[i + 2].id.Replace("expression", "assignment_statement");
                                gar.success = true;
                                return gar;
                            }
                        }
                    }
                    #endregion

                    #region operator_add_sub-item
                    if (candidate_symbols == "operator_add_sub-item") {
                        if (next_symbol.type == symbol_type.operator_mul_div) {
                            gar.success = false;
                            return gar;
                        }
                        gar.reduction_length = 2;
                        symbol temp = new symbol(symbol_type.expression);
                        temp.name = new_temp_var_name();
                        temp.id = symbol_list[i].id.Replace("operator_add_sub", "expression");
                        gar.reduction_result = temp;
                        if (symbol_list[i].name == "-") {
                            temp.value = -symbol_list[i + 1].value;
                            quaternion _q = new quaternion(quaternion_action.neg, symbol_list[i + 1].name, temp.name);
                            analyze_condition.quaternion_list.Add(_q);
                        } else {
                            temp.value = symbol_list[i + 1].value;
                        }
                        analyze_condition.symbol_table.Add(temp);
                        gar.success = true;
                        return gar;
                    }
                    #endregion

                    #region expression-operator_add_sub-item
                    if (candidate_symbols == "expression-operator_add_sub-item") {
                        if (next_symbol != null) {
                            if (next_symbol.type == symbol_type.operator_mul_div) {
                                gar.success = false;
                                return gar;
                            }
                        }
                        gar.reduction_length = 3;
                        symbol temp = new symbol(symbol_type.expression);
                        temp.name = new_temp_var_name();
                        temp.id = symbol_list[i].id;
                        gar.reduction_result = temp;
                        analyze_condition.symbol_table.Add(temp);
                        quaternion _q = new quaternion(symbol_list[i + 1].name == "+" ? quaternion_action.add : quaternion_action.sub, symbol_list[i].name, symbol_list[i + 2].name, temp.name);
                        analyze_condition.quaternion_list.Add(_q);
                        gar.success = true;
                        return gar;
                    }
                    #endregion
                    
                    #region item-operator_mul_div-factor
                    if (candidate_symbols == "item-operator_mul_div-factor") {
                        gar.reduction_length = 3;
                        symbol temp = new symbol(symbol_type.item);
                        temp.name = new_temp_var_name();
                        temp.id = symbol_list[i].id;
                        gar.reduction_result = temp;
                        analyze_condition.symbol_table.Add(temp);
                        quaternion _q = new quaternion(symbol_list[i + 1].name == "*" ? quaternion_action.mul : quaternion_action.div, symbol_list[i].name, symbol_list[i + 2].name, temp.name);
                        analyze_condition.quaternion_list.Add(_q);
                        gar.success = true;
                        return gar;
                    }
                    #endregion
                    
                    #region delimiter-expression-delimiter
                    if (candidate_symbols == "delimiter-expression-delimiter") {
                        gar.reduction_length = 3;
                        if (symbol_list[i].name == "(" && symbol_list[i + 2].name == ")") {
                            gar.reduction_result = symbol_list[i + 1];
                            gar.reduction_result.type = symbol_type.factor;
                            gar.reduction_result.id = symbol_list[i].id.Replace("delimiter", "factor");
                            gar.success = true;
                            return gar;
                        }
                    }
                    #endregion

                    #region IF-ELSE,WHILE-DO (reserved,reserved-condition-reserved,reserved-condition-reserved-statement)
                    if (candidate_symbols == "reserved") {
                        if (symbol_list[i].name == "WHILE") {
                            if (symbol_list[i].control_properties != null) {
                                gar.success = false;
                                return gar;
                            } else {
                                gar.reduction_length = 1;
                                gar.reduction_result = symbol_list[i];
                                gar.reduction_result.control_properties = new control_property();
                                gar.reduction_result.control_properties.quad = analyze_condition.next_quaternion_line_no;
                                return gar;
                            }
                        }
                    }

                    if (candidate_symbols == "reserved-condition-reserved") {
                        if ((symbol_list[i].name == "IF" && symbol_list[i + 2].name == "THEN") || (symbol_list[i].name == "WHILE" && symbol_list[i + 2].name == "DO")) {
                            if (symbol_list[i + 2].control_properties != null) {
                                gar.success = false;
                                return gar;
                            } else {
                                gar.reduction_length = 1;
                                gar.reduction_result = symbol_list[i + 2];
                                gar.reduction_result.control_properties = new control_property();
                                gar.reduction_result.control_properties.quad = analyze_condition.next_quaternion_line_no;
                                return gar;
                            }
                        }
                    }
                    if (candidate_symbols == "reserved-condition-reserved-statement") {
                        if (symbol_list[i].name == "IF" && symbol_list[i + 2].name == "THEN") {
                            try {
                                symbol_list[i + 1].control_properties.backpatch(control_property.list_type.true_list, symbol_list[i + 2].control_properties.quad);

                                gar.reduction_length = 4;
                                gar.reduction_result = new symbol(symbol_type.conditional_statement);
                                gar.reduction_result.id = symbol_list[i].id.Replace("reserved", "conditional_statement");
                                gar.reduction_result.control_properties = new control_property(true);
                                gar.reduction_result.control_properties.next_list.AddRange(symbol_list[i + 1].control_properties.false_list);
                                gar.reduction_result.control_properties.next_list.AddRange(symbol_list[i + 3].control_properties.next_list);

                                gar.success = true;
                                return gar;
                            } catch (Exception e) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): internal error.", e);
                                ex.Data["type"] = error_type.internal_code_error;
                                throw ex;
                            }
                        } else if (symbol_list[i].name == "WHILE" && symbol_list[i + 2].name == "DO") {
                            try {
                                symbol_list[i + 3].control_properties.backpatch(control_property.list_type.next_list, symbol_list[i].control_properties.quad);
                                symbol_list[i + 1].control_properties.backpatch(control_property.list_type.true_list, symbol_list[i + 2].control_properties.quad);

                                gar.reduction_length = 4;
                                gar.reduction_result = new symbol(symbol_type.loop_statement);
                                gar.reduction_result.id = symbol_list[i].id.Replace("reserved", "loop_statement");
                                gar.reduction_result.control_properties = new control_property(true);
                                gar.reduction_result.control_properties.next_list.AddRange(symbol_list[i + 1].control_properties.false_list);

                                analyze_condition.quaternion_list.Add(new quaternion(quaternion_action.jmp, "", "", symbol_list[i].control_properties.quad));

                                gar.success = true;
                                return gar;
                            } catch (Exception e) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): internal error.", e);
                                ex.Data["type"] = error_type.internal_code_error;
                                throw ex;
                            }
                        }
                    }
                    #endregion

                    #region expression-operator_rel-expression
                    if (candidate_symbols == "expression-operator_rel-expression") {
                        gar.reduction_length = 3;
                        gar.reduction_result = new symbol(symbol_type.condition);
                        gar.reduction_result.control_properties = new control_property();
                        gar.reduction_result.control_properties.make_list(control_property.list_type.true_list, analyze_condition.next_quaternion_line_no);
                        gar.reduction_result.control_properties.make_list(control_property.list_type.false_list, analyze_condition.next_quaternion_line_no + 1);

                        analyze_condition.quaternion_list.Add(new quaternion((quaternion_action)(C.op_rel.IndexOf(symbol_list[i + 1].name) + 1), symbol_list[i].name, symbol_list[i + 2].name));
                        analyze_condition.quaternion_list.Add(new quaternion(quaternion_action.jmp));

                        gar.success = true;
                        return gar;
                    }
                    #endregion

                    #region S1;S2 (loop_statement,conditional_statement,assignment_statement,statement-delimiter,statement-delimiter-statement)
                    if (new string[] { "loop_statement", "conditional_statement", "assignment_statement" }.Contains(candidate_symbols)) {

                        gar.reduction_result = symbol_list[i];
                        gar.reduction_result.type = symbol_type.statement;
                        string[] info = symbol_list[i].id.Split('-');
                        gar.reduction_result.id = "statement" + "-" + info[1] + "-" + info[2] + "-" + info[3];
                        if (candidate_symbols == "assignment_statement") gar.reduction_result.control_properties.next_list = new List<int>();
                        gar.reduction_length = 1;
                        gar.success = true;
                        return gar;
                    }

                    if (candidate_symbols == "statement-delimiter") {
                        if (symbol_list[i + 1].name == ";") {
                            if (symbol_list[i + 1].control_properties != null) {
                                symbol_list[i].control_properties.backpatch(control_property.list_type.next_list, symbol_list[i + 1].control_properties.quad);
                                gar.success = false;
                                return gar;
                            } else {
                                gar.reduction_length = 1;
                                gar.reduction_result = symbol_list[i + 1];
                                gar.reduction_result.control_properties = new control_property();
                                gar.reduction_result.control_properties.quad = analyze_condition.next_quaternion_line_no;
                                gar.success = true;
                                return gar;
                            }
                        }
                    }
                    if (candidate_symbols == "statement-delimiter-statement") {
                        if (symbol_list[i + 1].name == ";") {
                            try {

                                gar.reduction_length = 3;
                                gar.reduction_result = new symbol(symbol_type.statement);
                                gar.reduction_result.control_properties = new control_property(true);
                                gar.reduction_result.control_properties.next_list.AddRange(symbol_list[i + 2].control_properties.next_list);
                                gar.reduction_result.id = symbol_list[i].id;

                                gar.success = true;
                                return gar;
                            } catch (Exception e) {
                                string[] info = symbol_list[i].id.Split('-');
                                Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): internal error.", e);
                                ex.Data["type"] = error_type.internal_code_error;
                                throw ex;
                            }
                        }
                    }
                    #endregion

                    #region program_definition
                    if (candidate_symbols == "program_definition") {
                        gar.reduction_length = 1;
                        gar.reduction_result = null;
                        gar.success = true;
                        return gar;
                    }
                    #endregion

                    #region factor
                    if (candidate_symbols == "factor") {
                        gar.reduction_length = 1;
                        gar.reduction_result = symbol_list[i];
                        gar.reduction_result.type = symbol_type.item;
                        gar.reduction_result.id = gar.reduction_result.id.Replace("factor", "item");
                        gar.success = true;
                        return gar;
                    }
                    #endregion

                    #region identifier
                    if (candidate_symbols == "identifier") {
                        if (analyze_condition.in_constant_def_field) return gar;
                        if (!analyze_condition.symbol_table.Exists(x => x.name == symbol_list[i].name)) {
                            string[] info = symbol_list[i].id.Split('-');
                            Exception ex = new Exception("(line: " + info[1] + ", row: " + info[2] + "): unknown symbol \"" + symbol_list[i].name + "\" .");
                            ex.Data["type"] = error_type.unknown_symbol;
                            throw ex;
                        }
                        gar.reduction_length = 1;
                        gar.reduction_result = symbol_list[i];
                        gar.reduction_result.type = symbol_type.factor;
                        gar.reduction_result.id = gar.reduction_result.id.Replace("identifier", "factor");
                        gar.success = true;
                        return gar;
                    }
                    #endregion

                    #region item
                    if (candidate_symbols == "item") {
                        if (next_symbol != null) {
                            if (next_symbol.type == symbol_type.operator_mul_div) {
                                gar.success = false;
                                return gar;
                            }
                        }
                        gar.reduction_length = 1;
                        gar.reduction_result = symbol_list[i];
                        gar.reduction_result.type = symbol_type.expression;
                        gar.reduction_result.id = gar.reduction_result.id.Replace("item", "expression");
                        gar.success = true;
                        return gar;
                    }
                    #endregion

                    #region integer
                    if (candidate_symbols == "integer") {
                        gar.reduction_length = 1;
                        gar.reduction_result = symbol_list[i];
                        gar.reduction_result.type = symbol_type.factor;
                        gar.reduction_result.id = gar.reduction_result.id.Replace("integer", "factor");
                        gar.reduction_result.name = new_const_int_name();
                        symbol sym = new symbol(symbol_type.constant);
                        sym.name = gar.reduction_result.name;
                        sym.id = symbol_list[i].id;
                        sym.value = symbol_list[i].value;
                        analyze_condition.symbol_table.Add(sym);
                        gar.success = true;
                        return gar;
                    }
                    #endregion
                }
            //}
            return gar;
        }

        private static string new_temp_var_name() {
            return "_t_" + (analyze_condition.latest_temporay_variant_no++).ToString("D3");
        }

        private static string new_const_int_name() {
            return "_i_" + (analyze_condition.latest_temporay_variant_no++).ToString("D3");
        }

        private static bool is_temp_var(string name) {
            return name.StartsWith("t_");
        }

    }
}
