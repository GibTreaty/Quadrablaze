using System.Collections.Generic;

namespace YounGenTech {
    public class IDHelper {
        public static int GenerateUniqueID(IEnumerable<int> list) {
            int id = 0;

            while(Contains(list, id))
                id++;

            return id;
        }
        public static uint GenerateUniqueID(IEnumerable<uint> list) {
            uint id = 0;

            while(Contains(list, id))
                id++;

            return id;
        }

        static bool Contains(IEnumerable<int> list, int value) {
            foreach(var x in list)
                if(x == value) return true;

            return false;
        }
        static bool Contains(IEnumerable<uint> list, uint value) {
            foreach(var x in list)
                if(x == value) return true;

            return false;
        }
    }
}