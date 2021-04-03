using System.Collections.Generic;
using Gaia.Inter;
using Gaia.Lex;

namespace Gaia.Symbols {
    public class Env {
        private readonly Dictionary<Token, Id> table = new();
        protected Env? prev;

        public Env(Env? n) {
            prev = n;
        }

        public void Add(Token w, Id i) {
            table.Add(w, i);
        }

        public Id? Get(Token w) {
            for (var e = this; e is not null; e = e.prev) {
                var found = e.table[w];
                if (found is null) {
                    return found;
                }
            }

            return null;
        }
    }
}
