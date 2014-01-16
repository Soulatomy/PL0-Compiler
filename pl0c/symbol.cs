using System;
using System.Linq;
using System.Text;

namespace pl0c {
    enum symbol_type {
        integer,
        identifier,
        variant,
        constant,
        reserved,
        delimiter,
        operator_add_sub,
        operator_mul_div,
        operator_rel,
        loop_statement,
        conditional_statement,
        condition,
        factor,
        item,
        expression,
        assignment_statement,
        statement,
        complex_statement_start,
        complex_statement_end,
        null_statement,
        constant_definition,
        constant_declaration,
        variant_declaration,
        program_definition,
        program_name,
        others
    }
    
    class symbol {

        internal symbol_type type = symbol_type.others;
        internal int value = 0;
        internal string name = "";
        /// <summary>
        /// format: type-line-column-length
        /// </summary>
        internal string id = "";
        internal control_property control_properties = null;
        internal int hit_count = 0;
        /// <summary>
        /// read symbol from text
        /// </summary>
        /// <param name="col_start">(from 0) start position</param>
        /// <param name="src">source text</param>
        internal symbol(int col_start, string src, int line_id) {
            string reading = "";
            StringBuilder sb_read = new StringBuilder();
            int col = col_start;
            //bool reach_delimiter = false;
            while (col < src.Length) {
                if (col < src.Length - 1) {
                    reading = src.Substring(col, 2);
                } else {
                    reading = src.Substring(col, 1);
                }
                if (!C.delimiter.Contains(reading)) {
                    if (!C.delimiter.Contains(reading.Substring(0,1))) {
                        sb_read.Append(reading[0]);
                        col++;
                    } else {
                        //reach_delimiter = true;
                        reading = reading.Substring(0, 1);
                        break;
                    }
                } else {
                    //reach_delimiter = true;
                    break;
                }
            }
            /*
            if (reach_delimiter == false) {
                Exception ex = new Exception("(line: " + line_id.ToString() + ", column: " + col_start.ToString() + "): unexpected sentence ending.");
                ex.Data["skip-length"] = col_start - col;
                ex.Data["type"] = "error";
                throw ex;
            } else {
             * */
                if (sb_read.Length == 0) {
                    //delimiter
                    this.name = reading;
                    if (C.op_rel.Contains(reading)) { this.type = symbol_type.operator_rel; }
                    else if (reading == "+" || reading == "-") { this.type = symbol_type.operator_add_sub; }
                    else if (reading == "*" || reading == "/") { this.type = symbol_type.operator_mul_div; }
                    else { this.type = symbol_type.delimiter; }
                    this.id = make_id(col_start, line_id, this.type, reading.Length);
                } else {
                    //others
                    string word_read = sb_read.ToString();
                    if (C.reserved_symbol.Contains(word_read)) {
                        //reverved
                        if (word_read == "BEGIN") {
                            this.type = symbol_type.complex_statement_start;
                        } else if (word_read == "END") {
                            this.type = symbol_type.complex_statement_end;
                        } else {
                            this.type = symbol_type.reserved;
                        }
                        this.name = word_read;
                        this.id = make_id(col_start, line_id, this.type, word_read.Length);
                    } else if (C.alphabet.Contains(word_read[0])) {
                        foreach (char c in word_read) {
                            if (!C.alphabet.Contains(c)) {
                                Exception ex = new Exception("(line: " + (line_id + 1).ToString() + ", col: " + (col_start + 1).ToString() + "): unrecognized symbol " + word_read + ", contains wrong charactor.");
                                ex.Data["skip-length"] = word_read.Length;
                                ex.Data["type"] = error_type.unrecognized_symbol;
                                throw ex;
                            }
                        }
                        this.type = symbol_type.identifier;
                        this.name = word_read;
                        this.id = make_id(col_start, line_id, this.type, word_read.Length);
                    } else if (C.number.Contains(word_read[0])) {
                        try {
                            this.value = int.Parse(word_read);
                        } catch (Exception e) {
                            Exception ex = new Exception("(line: " + (line_id + 1).ToString() + ", col: " + (col_start + 1).ToString() + "): " + e.Message, e);
                            ex.Data["skip-length"] = word_read.Length;
                            ex.Data["type"] = error_type.integer_parse_error;
                            throw ex;
                        }
                        this.type = symbol_type.integer;
                        this.name = word_read;
                        this.id = make_id(col_start, line_id, this.type, word_read.Length);
                    } else {
                        Exception ex = new Exception("(line: " + (line_id + 1).ToString() + ", col: " + (col_start + 1).ToString() + "): unrecognized symbol " + word_read + ".");
                        ex.Data["skip-length"] = word_read.Length;
                        ex.Data["type"] = error_type.unrecognized_symbol;
                        throw ex;
                    }
                }
            /*}*/
        }
        internal symbol(symbol_type _type) {
            this.type = _type;
        }

        private string make_id (int col,int line,symbol_type st,int length){
            return st.ToString("G") + "-" + (line + 1).ToString() + "-" + (col + 1).ToString() + "-" + length.ToString();
        }
    }
}
