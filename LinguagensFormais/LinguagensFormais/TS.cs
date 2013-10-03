using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompiladoresTrabalho
{
    public enum TipoEstrutura
    {
        VariavelComum = 0,
        VariavelParametro = 1,
        VariavelParametroRef = 2,
        Procedimento = 3,
        Funcao = 4        
    }

    public class TSymbol
    {
        public string id;
        public string context;
        public int tipo;
        public TipoEstrutura tipoEstrutura;

        private string _valor = null;
        public String Valor
        {
            set
            {
                String v = value;
                if(value != null)
                {
                    v = String.Join("", v.Split(' '));
                }

                this._valor = v;
            }

            get
            {
                return this._valor;
            }
        }

        public TSymbol() { }

        public TSymbol(string _id, int _tipo, string _context, TipoEstrutura _tipoEstrutura)
        {
            this.id = _id;
            this.tipo = _tipo;
            this.context = _context;
            this.tipoEstrutura = _tipoEstrutura;
        }

        public TSymbol(string _id, int _tipo)
        {
            this.id = _id;
            this.tipo = _tipo;
            this.context = TS.MainContext;
            this.tipoEstrutura = TipoEstrutura.VariavelComum;
        }

        public override string ToString()
        {
            return String.Format("id({0}) | context({1}) | tipo({2}) | tipoEstrutura({3}) | valor({4})", this.id, this.context, LexMap.TokenGetNome(tipo), this.tipoEstrutura, this.Valor);
        }
    }

    public class TS
    {
        public static string CurrentContext = String.Empty;
        public static string MainContext = "";

        private static TS _Instance { get; set; }
        public static TS Instance 
        {
            get
            {
                if (TS._Instance == null)
                {
                    TS._Instance = new TS();
                }

                return TS._Instance;
            }
        }

        private List<TSymbol> TabelaDeSimbolos;

        public TS()
        {
            this.TabelaDeSimbolos = new List<TSymbol>();
        }

        public void Add(TSymbol t)
        {
            this.TabelaDeSimbolos.Add(t);
        }

        public TSymbol GetSymbol(string id)
        {
            foreach (TSymbol item in this.TabelaDeSimbolos)
            {
                if (item.id.Equals(id, StringComparison.InvariantCultureIgnoreCase) && item.context.Equals(CurrentContext))
                {
                    return item;
                }
            }

            foreach (TSymbol item in this.TabelaDeSimbolos)
            {
                if (item.id.Equals(id, StringComparison.InvariantCultureIgnoreCase) && item.context.Equals(TS.MainContext))
                {
                    return item;
                }
            }

            return null;
        }

        public List<TSymbol> GetChildSymbols(string id)
        {
            List<TSymbol> listSymbols = new List<TSymbol>();

            foreach (TSymbol item in this.TabelaDeSimbolos)
            {
                if (item.context.Equals(id) && 
                    ((item.tipoEstrutura == TipoEstrutura.VariavelParametro) ||
                     (item.tipoEstrutura == TipoEstrutura.VariavelParametroRef)))
                {
                    listSymbols.Add(item);
                }
            }

            return listSymbols;
        }

        public bool ExistsVariavelComumParametro(string id)
        {
            foreach (TSymbol item in this.TabelaDeSimbolos)
            {
                if (item.id.Equals(id) && 
                    (item.context.Equals(CurrentContext) || 
                     (item.tipoEstrutura == TipoEstrutura.Procedimento) || 
                     (item.tipoEstrutura == TipoEstrutura.Funcao)))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ExistsProcedimentoFuncaoVariavelComum(string id)
        {
            foreach (TSymbol item in this.TabelaDeSimbolos)
            {
                if (item.id.Equals(id) && item.context.Equals(CurrentContext) && 
                    ((item.tipoEstrutura == TipoEstrutura.Procedimento) ||
                     (item.tipoEstrutura == TipoEstrutura.Funcao) ||
                     (item.tipoEstrutura == TipoEstrutura.VariavelComum)
                    )
                   )
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasValue(string id)
        {
            foreach (TSymbol item in this.TabelaDeSimbolos)
            {
                if (item.id.Equals(id))
                {
                    if (item.Valor != null)
                    {
                        return true;
                    }

                    return false;
                }
            }

            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (TSymbol item in this.TabelaDeSimbolos)
            {
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }
    }
}
