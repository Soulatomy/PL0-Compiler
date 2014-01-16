using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pl0c {
    class control_property {
        
        internal List<int> true_list = null;
        internal List<int> false_list = null;
        internal List<int> next_list = null;
        internal int quad = -1;

        [FlagsAttribute]
        internal enum list_type {
            true_list = 1,
            false_list = 2,
            next_list = 4
        }


        internal control_property(bool need_next_list = false) {
            if (need_next_list) this.next_list = new List<int>();
        }

        internal void backpatch(list_type backpatch_list, int backpatch_quad) {
            if (backpatch_list.HasFlag(list_type.true_list)) 
                foreach (int item in this.true_list) analyze_condition.quaternion_list[item].next = backpatch_quad;

            if (backpatch_list.HasFlag(list_type.false_list)) 
                foreach (int item in this.false_list) analyze_condition.quaternion_list[item].next = backpatch_quad;
            
            if(backpatch_list.HasFlag(list_type.next_list))
                foreach (int item in this.next_list) analyze_condition.quaternion_list[item].next = backpatch_quad;
        }

        internal void make_list(list_type backpatch_list, int first_value) {
            if (backpatch_list.HasFlag(list_type.true_list)) this.true_list = new List<int>() { first_value };

            if (backpatch_list.HasFlag(list_type.false_list)) this.false_list = new List<int>() { first_value };

            if (backpatch_list.HasFlag(list_type.next_list)) this.next_list = new List<int>() { first_value };
        }


    }
}
