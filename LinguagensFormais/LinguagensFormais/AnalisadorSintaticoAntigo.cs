using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguagensFormais
{
    public class AnalisadorSintaticoAntigo
    {
        public String Execute()
        {
            if (LineManager.Instance.ReadLine())
            {
                String AlgCod = String.Empty;

                AnalisadorLexico.Analisar();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["ALGORITMO"])
                {
                    throw new AnalisadorException("Palavra chave Algoritmo não encontrada");
                }

                this.Algoritmo(out AlgCod);

                return AlgCod;
            }

            return "";
        }

        private void Bibliotecas(out String bibliotecaCod)
        {
            bibliotecaCod = @"#include <stdio.h>" + Environment.NewLine + "#include <stdlib.h>" + Environment.NewLine;
        }

        private void NomeDoAlgoritmo(out String nomeAlgoritmoCod)
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["STRING"])
            {
                nomeAlgoritmoCod = TokenManager.Instance.TokenSymbol.Trim('"');
            }
            else
            {
                nomeAlgoritmoCod = "xmain";
            }
        }

        public bool TipoVar(out String tipoVarCod)
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["INTEIRO"])
            {
                tipoVarCod = "int";
            }
            else if (TokenManager.Instance.TokenCode == LexMap.Consts["REAL"])
            {
                tipoVarCod = "float";
            }
            else if (TokenManager.Instance.TokenCode == LexMap.Consts["LOGICO"])
            {
                tipoVarCod = "int";
            }
            else
            {
                throw new AnalisadorException(
                    "Tipo de variável ou função não foi encontrado ou é inválido.", 
                    LineManager.Instance.PosStartToken, 
                    LineManager.Instance.LineIndex, 
                    TokenManager.Instance.TokenSymbol);
            }

            return true;
        }

        public bool ListaVar(out String listaVarCod, List<String> VarList)
        {
            listaVarCod = String.Empty;

            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
            {
                AnalisadorLexico.Analisar();

                if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
                {
                    String identificadorCod;
                    identificadorCod = TokenManager.Instance.TokenSymbol;
                    VarList.Add(identificadorCod);

                    String listaVarCodRecur;
                    this.ListaVar(out listaVarCodRecur, VarList);

                    listaVarCod = ", " + identificadorCod + listaVarCodRecur;

                    return true;
                }
                else
                {
                    LineManager.Instance.LinesOut.Add("Um identificador era esperado");
                    return false;
                }
            }
            return false;
        }

        public bool DeclaraVar(out String declaraVarCod)
        {
            String identificadorCod;
            identificadorCod = TokenManager.Instance.TokenSymbol;

            String listaVarCod;

            List<String> VarList = new List<String>();
            VarList.Add(identificadorCod);

            this.ListaVar(out listaVarCod, VarList);

            String tipoVarCod;
            if (TokenManager.Instance.TokenCode != LexMap.Consts["DOISPONTOS"])
            {
                throw new AnalisadorException("Esperado dois pontos.");
            }

            this.TipoVar(out tipoVarCod);
            int tipo = TokenManager.Instance.TokenCode;

            foreach (String item in VarList)
            {
                if (!TS.Instance.ExistsProcedimentoFuncaoVariavelComum(item.Trim()))
                {
                    TS.Instance.Add(new TSymbol(item.Trim(), tipo));
                }
                else
                {
                    throw new AnalisadorException(String.Format("Variável já declarada. Identificador {0} já existe na TabSymbol para o contexto {1}.", item.Trim(), TS.MainContext));
                }
            }

            String listaDeclaracoesCod;
            this.ListaDeclaracoes(out listaDeclaracoesCod);

            declaraVarCod = tipoVarCod + " " + identificadorCod + listaVarCod + ";" + Environment.NewLine + listaDeclaracoesCod;

            return true;
        }

        public bool ListaDeclaracoes(out String listaDeclaracoesCod)
        {
            AnalisadorLexico.Analisar();

            listaDeclaracoesCod = String.Empty;

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"]) 
            {
                String declaraVarCod;
                this.DeclaraVar(out declaraVarCod);

                listaDeclaracoesCod = declaraVarCod;
            }
            else if (TokenManager.Instance.TokenCode == LexMap.Consts["PROCEDIMENTO"]) 
            {
                String procedimentoCod;
                this.Procedimento(out procedimentoCod);

                listaDeclaracoesCod = procedimentoCod;
            }
            else if (TokenManager.Instance.TokenCode == LexMap.Consts["FUNCAO"])
            {
                String funcaoCod;
                this.Funcao(out funcaoCod);

                listaDeclaracoesCod = funcaoCod;
            }
            
            return true;
        }

        private void Funcao(out string funcaoCod)
        {
            String nomeFuncaoCod;
            NomeDaFuncao(out nomeFuncaoCod);

            TS.CurrentContext = nomeFuncaoCod;

            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
            {
                throw new AnalisadorException(String.Format("Era esperado o token {0}", LexMap.TokenGetNome(LexMap.Consts["ABREPAR"])));
            }

            String listaParametrosProcFunc;
            ListaParametrosProcFunc(out listaParametrosProcFunc);

            if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
            {
                throw new AnalisadorException(String.Format("Era esperado o token {0}", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"])));
            }

            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["DOISPONTOS"])
            {
                throw new AnalisadorException(String.Format("Esperado token {0} na função {1}", LexMap.TokenGetNome(LexMap.Consts["DOISPONTOS"]), nomeFuncaoCod));
            }

            String tipoVarCod;
            this.TipoVar(out tipoVarCod);
            int tipo = TokenManager.Instance.TokenCode;

            TSymbol tsy = TS.Instance.GetSymbol(nomeFuncaoCod);
            tsy.tipo = tipo;

            String funcaoVariaveisCod;
            FuncaoVariaveis(out funcaoVariaveisCod);

            funcaoCod = String.Format("{0} {1}({2}){3}{4}", tipoVarCod, nomeFuncaoCod, listaParametrosProcFunc, Environment.NewLine, funcaoVariaveisCod);
        }

        private void NomeDaFuncao(out String nomeFuncaoCod)
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
            {
                nomeFuncaoCod = TokenManager.Instance.TokenSymbol;

                if (!TS.Instance.ExistsProcedimentoFuncaoVariavelComum(nomeFuncaoCod.Trim()))
                {
                    TS.Instance.Add(new TSymbol(nomeFuncaoCod.Trim(), LexMap.Consts["VAZIO"], TS.MainContext, TipoEstrutura.Funcao));
                }
                else
                {
                    throw new AnalisadorException(String.Format("Idenficador já declarado. Identificador {0} já existe na TabSymbol para o contexto {1}.", nomeFuncaoCod.Trim(), TS.CurrentContext));
                }
            }
            else
            {
                throw new AnalisadorException("A assinatura da funcao necessita de um identificador.");
            }
        }

        private void FuncaoVariaveis(out string funcaoVariaveisCod)
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VAR"])
            {
                String listaDeclaracoesProcFunc;
                ListaDeclaracoesProcFunc(out listaDeclaracoesProcFunc);

                if (TokenManager.Instance.TokenCode == LexMap.Consts["INICIO"])
                {
                    String funcaoComandosCod;
                    FuncaoComandos(out funcaoComandosCod);

                    funcaoVariaveisCod = "{" + Environment.NewLine + listaDeclaracoesProcFunc + Environment.NewLine + funcaoComandosCod;
                }
                else
                {
                    throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["INICIO"]));
                }
            }
            else
            {
                if (TokenManager.Instance.TokenCode == LexMap.Consts["INICIO"])
                {
                    String funcaoComandosCod;
                    FuncaoComandos(out funcaoComandosCod);

                    funcaoVariaveisCod = "{" + Environment.NewLine + funcaoComandosCod;
                }
                else
                {
                    throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["INICIO"]) + " ou token " + LexMap.TokenGetNome(LexMap.Consts["VAR"]));
                }
            }
        }

        private void FuncaoComandos(out string funcaoComandosCod)
        {
            String ListaDeComandosCod;
            this.ListaDeComandos(out ListaDeComandosCod);

            if (TokenManager.Instance.TokenCode != LexMap.Consts["FIMFUNCAO"])
            {
                throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["FIMFUNCAO"]));
            }

            TS.CurrentContext = TS.MainContext;

            String listaDeclaracoesCod;
            this.ListaDeclaracoes(out listaDeclaracoesCod);

            funcaoComandosCod = ListaDeComandosCod + "}" + Environment.NewLine + listaDeclaracoesCod;
        }

        private void Procedimento(out string procedimentoCod)
        {
            procedimentoCod = "void ";

            String nomeProcedimentoCod;
            NomeDoProcedimento(out nomeProcedimentoCod);

            TS.CurrentContext = nomeProcedimentoCod;

            procedimentoCod += nomeProcedimentoCod;

            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
            {
                throw new AnalisadorException(String.Format("Era esperado o token {0}", LexMap.TokenGetNome(LexMap.Consts["ABREPAR"])));
            }

            procedimentoCod += "(";

            String listaParametrosProcFunc;
            ListaParametrosProcFunc(out listaParametrosProcFunc);

            procedimentoCod += listaParametrosProcFunc;

            if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
            {
                throw new AnalisadorException(String.Format("Era esperado o token {0}", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"])));
            }

            procedimentoCod += ")" + Environment.NewLine;

            String procedimentoVariaveisCod;
            ProcedimentoVariaveis(out procedimentoVariaveisCod);

            procedimentoCod += procedimentoVariaveisCod;
        }

        private void ListaParametrosProcFunc(out string listaParametrosProcFunc)
        {
            AnalisadorLexico.Analisar();

            listaParametrosProcFunc = String.Empty;

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VAR"])
            {
                AnalisadorLexico.Analisar();

                if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
                {
                    String identificadorCod = TokenManager.Instance.TokenSymbol;

                    String parametrosProcFuncCod;
                    ParametrosProcFunc(out parametrosProcFuncCod, "*", identificadorCod);

                    listaParametrosProcFunc = parametrosProcFuncCod;
                }
                else
                {
                    throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["ID"]));
                }
            }
            else if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
            {
                String identificadorCod = TokenManager.Instance.TokenSymbol;

                String parametrosProcFuncCod;
                ParametrosProcFunc(out parametrosProcFuncCod, String.Empty, identificadorCod);

                listaParametrosProcFunc = parametrosProcFuncCod;
            }
        }

        private void ParametrosProcFunc(out string parametrosProcFunc, string reference, string identificadorCod)
        {
            String listaVarCod;

            List<String> VarList = new List<String>();
            VarList.Add(identificadorCod);

            this.ListaVar(out listaVarCod, VarList);

            if (TokenManager.Instance.TokenCode != LexMap.Consts["DOISPONTOS"])
            {
                throw new AnalisadorException("Esperado dois pontos.");
            }

            String tipoVarCod;
            this.TipoVar(out tipoVarCod);
            int tipo = TokenManager.Instance.TokenCode;

            foreach (String item in VarList)
            {
                if (!TS.Instance.ExistsVariavelComumParametro(item.Trim()))
                {
                    TipoEstrutura tipoEstruturaParametro = TipoEstrutura.VariavelParametroRef;
                    if(String.IsNullOrEmpty(reference))
                        tipoEstruturaParametro = TipoEstrutura.VariavelParametro;

                    TS.Instance.Add(new TSymbol(item.Trim(), tipo, TS.CurrentContext, tipoEstruturaParametro));
                }
                else
                {
                    throw new AnalisadorException(String.Format("Variável parametro de procedimento já declarada. Identificador {0} já existe na TabSymbol para o contexto {1}.", item.Trim(), TS.CurrentContext));
                }
            }

            String listaParametrosProcFunc1;
            this.ListaParametrosProcFunc(out listaParametrosProcFunc1);

            String othersIds = String.Empty;
            for (int i = 1; i < VarList.Count; i++)
            {
                othersIds += String.Concat(", " + tipoVarCod + " " + reference + VarList[i]);
            }

            if (!String.IsNullOrEmpty(listaParametrosProcFunc1))
                listaParametrosProcFunc1 = String.Concat(", ", listaParametrosProcFunc1);

            parametrosProcFunc = tipoVarCod + " " + reference + identificadorCod + othersIds + listaParametrosProcFunc1;            
        }

        private void ProcedimentoVariaveis(out string procedimentoVariaveisCod)
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VAR"])
            {
                String listaDeclaracoesProcFunc;
                ListaDeclaracoesProcFunc(out listaDeclaracoesProcFunc);

                if (TokenManager.Instance.TokenCode == LexMap.Consts["INICIO"])
                {
                    String procedimentoComandosCod;
                    ProcedimentoComandos(out procedimentoComandosCod);

                    procedimentoVariaveisCod = "{" + Environment.NewLine + listaDeclaracoesProcFunc + Environment.NewLine + procedimentoComandosCod;
                }
                else
                {
                    throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["INICIO"]));
                }
            }
            else
            {
                if (TokenManager.Instance.TokenCode == LexMap.Consts["INICIO"])
                {
                    String procedimentoComandosCod;
                    ProcedimentoComandos(out procedimentoComandosCod);

                    procedimentoVariaveisCod = "{" + Environment.NewLine + procedimentoComandosCod;
                }
                else
                {
                    throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["INICIO"]) + " ou token " + LexMap.TokenGetNome(LexMap.Consts["VAR"]));
                }
            }
        }

        private void ListaDeclaracoesProcFunc(out string listaDeclaracoesProcFunc)
        {
            AnalisadorLexico.Analisar();

            listaDeclaracoesProcFunc = String.Empty;

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
            {
                String identificadorCod = TokenManager.Instance.TokenSymbol;

                String listaVarCod;

                List<String> VarList = new List<String>();
                VarList.Add(identificadorCod);

                this.ListaVar(out listaVarCod, VarList);

                String tipoVarCod;
                if (TokenManager.Instance.TokenCode != LexMap.Consts["DOISPONTOS"])
                {
                    throw new AnalisadorException("Esperado dois pontos.");
                }

                this.TipoVar(out tipoVarCod);
                int tipo = TokenManager.Instance.TokenCode;

                foreach (String item in VarList)
                {
                    if (!TS.Instance.ExistsVariavelComumParametro(item.Trim()))
                    {
                        TS.Instance.Add(new TSymbol(item.Trim(), tipo, TS.CurrentContext, TipoEstrutura.VariavelComum));
                    }
                    else
                    {
                        throw new AnalisadorException(String.Format("Variável do procedimento já declarada. Identificador {0} já existe na TabSymbol para o contexto {1}.", item.Trim(), TS.CurrentContext));
                    }
                }

                String listaDeclaracoesProcFunc1;
                this.ListaDeclaracoesProcFunc(out listaDeclaracoesProcFunc1);

                listaDeclaracoesProcFunc = "\t" + tipoVarCod + " " + identificadorCod + listaVarCod + ";" + Environment.NewLine + listaDeclaracoesProcFunc1;
            }
        }

        private void ProcedimentoComandos(out string procedimentoComandosCod)
        {
            String ListaDeComandosCod;
            this.ListaDeComandos(out ListaDeComandosCod);

            if (TokenManager.Instance.TokenCode != LexMap.Consts["FIMPROCEDIMENTO"])
            {
                throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["FIMPROCEDIMENTO"]));
            }

            TS.CurrentContext = TS.MainContext;

            String listaDeclaracoesCod;
            this.ListaDeclaracoes(out listaDeclaracoesCod);

            procedimentoComandosCod = ListaDeComandosCod + "}" + Environment.NewLine + listaDeclaracoesCod;
        }

        private void NomeDoProcedimento(out String nomeProcedimentoCod)
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
            {
                nomeProcedimentoCod = TokenManager.Instance.TokenSymbol;

                if (!TS.Instance.ExistsProcedimentoFuncaoVariavelComum(nomeProcedimentoCod.Trim()))
                {
                    TS.Instance.Add(new TSymbol(nomeProcedimentoCod.Trim(), LexMap.Consts["VAZIO"], TS.MainContext, TipoEstrutura.Procedimento));
                }
                else
                {
                    throw new AnalisadorException(String.Format("Identificador já declarado. Identificador {0} já existe na TabSymbol para o contexto {1}.", nomeProcedimentoCod.Trim(), TS.CurrentContext));
                }
            }
            else
            {
                throw new AnalisadorException("A assinatura do procedimento necessita de um identificador.");
            }
        }

        public bool Declaracoes(out String declaracoesCod)
        {
            AnalisadorLexico.Analisar();

            declaracoesCod = String.Empty;

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VAR"])
            {
                String listaDeclaracoesCod;
                this.ListaDeclaracoes(out listaDeclaracoesCod);

                declaracoesCod = listaDeclaracoesCod;

                return true;
            }
            else
            {
                LineManager.Instance.LinesOut.Add("A palavra reservada -var- era esperada.");

                return false;
            }
        }

        #region expv1
        /*
        public void ExpArit_F(out String expArit_FCod)
        {
            AnalisadorLexico.Analisar();

            string tk = TokenManager.Instance.TokenSymbol;

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["CONSTINTEIRO"] |
                TokenManager.Instance.TokenCode == LexMap.Consts["CONSTREAL"])
            {
                if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
                {
                    if (!TS.Instance.Exists(tk))
                    {
                        throw new AnalisadorException(
                            String.Format("A variável {0} não foi declarada.", tk));
                    }

                    
                    //if (!TS.Instance.HasValue(tk))
                    //{
                    //   throw new AnalisadorException(
                    //        String.Format("A variável {0} não instanciada.", tk));
                    //}
                }


                expArit_FCod = " " + tk;
            }
            else if (TokenManager.Instance.TokenCode == LexMap.Consts["ABREPAR"])
            {
                String ExpArit_Cod;
                this.ExpArit(out ExpArit_Cod);
                expArit_FCod = " (" + ExpArit_Cod + " )";

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException(
                       String.Format("Era esperado o token {0}", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"])),
                       LineManager.Instance.PosStartToken,
                       LineManager.Instance.LineIndex,
                       TokenManager.Instance.TokenCode,
                       TokenManager.Instance.TokenSymbol);
                }
            }
            else
            {
                throw new AnalisadorException(
                   "O valor da expressão aritmética (F) não é válido",
                   LineManager.Instance.PosStartToken,
                   LineManager.Instance.LineIndex,
                   TokenManager.Instance.TokenCode,
                   TokenManager.Instance.TokenSymbol);
            }
        }

        public void ExpArit_X(string Xh, out String expArit_XCod)
        {
            AnalisadorLexico.Analisar();
            int opType = TokenManager.Instance.TokenCode;
            string op = "";
            String ExpArit_FCod = "";
            String ExpArit_X1Cod = "";
            bool is_valid_flag = false;

            expArit_XCod = "";

            if (opType == LexMap.Consts["MULTIPLICACAO"])
            {
                op = "*";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["DIVISAO"])
            {
                op = "/";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MOD"])
            {
                op = "%";
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                this.ExpArit_F(out ExpArit_FCod);

                string X1h = Xh + " " + op + ExpArit_FCod;

                this.ExpArit_X(X1h, out ExpArit_X1Cod);

                expArit_XCod = ExpArit_X1Cod;
            }
            else
            {
                expArit_XCod = Xh;
            }
        }

        public void ExpArit_R(String Rh, out String expArit_RCod)
        {
            //AnalisadorLexico.Analisar();
            int opType = TokenManager.Instance.TokenCode;
            string op = "";
            String ExpArit_TCod = "";
            String ExpArit_R1Cod = "";
            bool is_valid_flag = false;

            expArit_RCod = "";

            if (opType == LexMap.Consts["MAIS"])
            {
                op = "+";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MENOS"])
            {
                op = "-";
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                this.ExpArit_T(out ExpArit_TCod);

                string R1h = Rh + " " + op + ExpArit_TCod;

                this.ExpArit_R(R1h, out ExpArit_R1Cod);
                expArit_RCod = ExpArit_R1Cod;
            }
            else
            {
                expArit_RCod = Rh;
            }
        }

        public void ExpArit_T(out String expArit_TCod)
        {
            String ExpArit_FCod;
            this.ExpArit_F(out ExpArit_FCod);

            string Xh = ExpArit_FCod;

            String ExpArit_XCod;
            this.ExpArit_X(Xh, out ExpArit_XCod);

            expArit_TCod = ExpArit_XCod;
        }

        public void ExpArit(out String expAritCod)
        {
            String ExpArit_TCod;
            String ExpArit_RCod;

            this.ExpArit_T(out ExpArit_TCod);

            String Rh = ExpArit_TCod;
            this.ExpArit_R(Rh, out ExpArit_RCod);

            expAritCod = ExpArit_RCod;
        }

        public void Atribuicao(out String atribuicaoCod)
        {
            string ExpAritCod;
            this.ExpArit(out ExpAritCod);

            atribuicaoCod = " =" + ExpAritCod + ";" + Environment.NewLine;
        }*/

        #endregion expv1

        #region expv2

        public void Exp(out String ExpCod, out Int32 ExpTipo)
        {
            String Tval, Rh, Rs;
            Int32 Ttipo, Rth, Rts;
            this.ExpT(out Tval, out Ttipo);

            Rh = Tval;
            Rth = Ttipo;

            this.ExpR(Rh, Rth, out Rs, out Rts);

            ExpCod = Rs;
            ExpTipo = Rts;
        }

        public void ExpR(String Rh, Int32 Rth, out String Rs, out Int32 Rts)
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            String op = TokenManager.Instance.TokenSymbol;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["OU"])
            {
                op = "||";
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                int cbool = LexMap.Consts["LOGICO"];

                String Tval;
                Int32 Ttipo;
                this.ExpT(out Tval, out Ttipo);

                String R1h, R1s;
                Int32 R1th, R1ts;

                R1h = Rh + op + Tval;

                R1th = 0;

                if (Rth == cbool && Ttipo == cbool)
                    R1th = cbool;

                if (R1th == 0)
                    throw new AnalisadorException(
                            String.Format("Expressões incompatíveis(V): {0}{1}{2}",
                                LexMap.TokenGetNome(Rth), op, LexMap.TokenGetNome(Ttipo)));

                this.ExpR(R1h, R1th, out R1s, out R1ts);

                Rs = R1s;
                Rts = R1ts;
            }
            else
            {
                Rs = Rh;
                Rts = Rth;
            }
        }

        public void ExpT(out String Tval, out Int32 Ttipo)
        {
            String Fval, Uh, Us;
            Int32 Ftipo, Uth, Uts;
            this.ExpF(out Fval, out Ftipo);

            Uh = Fval;
            Uth = Ftipo;

            this.ExpU(Uh, Uth, out Us, out Uts);
            LineManager.Instance.ResetToLastPos();

            Tval = Us;
            Ttipo = Uts;
        }

        public void ExpF(out String Fval, out Int32 Ftipo)
        {
            String Gval;
            Int32 Gtipo;
            this.ExpG(out Gval, out Gtipo);

            Fval = Gval;
            Ftipo = Gtipo;
        }

        public void ExpU(String Uh, Int32 Uth, out String Us, out Int32 Uts)
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            String op = TokenManager.Instance.TokenSymbol;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["E"])
            {
                op = "&&";
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                int cbool = LexMap.Consts["LOGICO"];

                String Fval;
                Int32 Ftipo;
                this.ExpF(out Fval, out Ftipo);

                String U1h, U1s;
                Int32 U1th, U1ts;
                
                U1h = Uh + op + Fval;

                U1th = 0;

                if(Uth == cbool && Ftipo == cbool)
                    U1th = cbool;

                if(U1th == 0)
                    throw new AnalisadorException(
                            String.Format("Expressões incompatíveis(V): {0}{1}{2}",
                                LexMap.TokenGetNome(Uth), op, LexMap.TokenGetNome(Ftipo)));

                this.ExpU(U1h, U1th, out U1s, out U1ts);

                Us = U1s;
                Uts = U1ts;
            }
            else
            {
                Us = Uh;
                Uts = Uth;
            }
        }

        public void ExpG(out String Gval, out Int32 Gtipo)
        {
            AnalisadorLexico.Analisar();
            if(TokenManager.Instance.TokenCode == LexMap.Consts["NAO"])
            {
                String G1val;
                Int32 G1tipo;
                this.ExpG(out G1val, out G1tipo);

                if(G1tipo != LexMap.Consts["LOGICO"])
                    throw new AnalisadorException(
                            String.Format("Expressões incompatíveis(X): {0}{1}",
                                "!", LexMap.TokenGetNome(G1tipo)));

                Gval = "!" + G1val;
                Gtipo = G1tipo;
            }
            else
            {
                //ACHO
                LineManager.Instance.ResetToLastPos();

                String Hval, Vh, Vs;
                Int32 Htipo, Vth, Vts;
                this.ExpH(out Hval, out Htipo);

                Vh = Hval;
                Vth = Htipo;
                
                this.ExpV(Vh, Vth, out Vs, out Vts);
                LineManager.Instance.ResetToLastPos();

                Gval = Vs;
                Gtipo = Vts;
            }
        }

        public void ExpV(String Vh, Int32 Vth, out String Vs, out Int32 Vts)
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            String op = TokenManager.Instance.TokenSymbol;
            bool is_valid_flag = false;
            bool accept_bool = false;

            if (opType == LexMap.Consts["MAIOR"])
            {
                op = ">";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["IGUAL"])
            {
                op = "==";
                is_valid_flag = true;
                accept_bool = true;
            }

            if (opType == LexMap.Consts["MENOR"])
            {
                op = "<";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MAIORIGUAL"])
            {
                op = ">=";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["DIFERENTE"])
            {
                op = "!=";
                is_valid_flag = true;
                accept_bool = true;
            }

             if (opType == LexMap.Consts["MENORIGUAL"])
            {
                op = "<=";
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                int cint = LexMap.Consts["INTEIRO"];
                int creal = LexMap.Consts["REAL"];
                int cbool = LexMap.Consts["LOGICO"];


                String Hval;
                Int32 Htipo;
                this.ExpH(out Hval, out Htipo);

                String V1h, V1s;
                Int32 V1th, V1ts;
                
                V1h = Vh + op + Hval;

                V1th = 0;

                if(Vth == cint && Htipo == cint ||
                    Vth == creal && Htipo == cint ||
                    Vth == cint && Htipo == creal ||
                    (Vth == cbool && Htipo == cbool && accept_bool == true)
                )
                {
                    V1th = cbool;
                }

                if(V1th == 0)
                    throw new AnalisadorException(
                            String.Format("Expressões incompatíveis(V): {0}{1}{2}",
                                LexMap.TokenGetNome(Vth), op, LexMap.TokenGetNome(Htipo)));

                this.ExpV(V1h, V1th, out V1s, out V1ts);

                Vs = V1s;
                Vts = V1ts;
            }
            else
            {
                Vs = Vh;
                Vts = Vth;
            }
        }

        public void ExpH(out String Hval, out Int32 Htipo)
        {
            String Jval, Xh, Xs;
            Int32 Jtipo, Xth, Xts;
            this.ExpJ(out Jval, out Jtipo);

            Xh = Jval;
            Xth = Jtipo;

            this.ExpX(Xh, Xth, out Xs, out Xts);
            LineManager.Instance.ResetToLastPos();

            Hval = Xs;
            Htipo = Xts;
        }

        public void ExpJ(out String Jval, out Int32 Jtipo)
        {
            String Kval, Yh, Ys;
            Int32 Ktipo, Yth, Yts;
            this.ExpK(out Kval, out Ktipo);

            Yh = Kval;
            Yth = Ktipo;

            this.ExpY(Yh, Yth, out Ys, out Yts);
            LineManager.Instance.ResetToLastPos();

            Jval = Ys;
            Jtipo = Yts;
        }

        public void ExpX(String Xh, Int32 Xth, out String Xs, out Int32 Xts)
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            String op = TokenManager.Instance.TokenSymbol;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["MAIS"])
            {
                op = "+";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MENOS"])
            {
                op = "-";
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                int cint = LexMap.Consts["INTEIRO"];
                int creal = LexMap.Consts["REAL"];


                String Jval;
                Int32 Jtipo;
                this.ExpJ(out Jval, out Jtipo);

                String X1h, X1s;
                Int32 X1th, X1ts;
                
                X1h = Xh + op + Jval;

                X1th = 0;

                if(Xth == cint && Jtipo == cint)
                    X1th = cint;

                if(Xth == creal)
                    X1th = creal;

                if(Jtipo == creal)
                    X1th = creal;

                if(X1th == 0)
                    throw new AnalisadorException(
                            String.Format("Expressões incompatíveis(X): {0}{1}{2}",
                                LexMap.TokenGetNome(Xth), op, LexMap.TokenGetNome(Jtipo)));

                this.ExpX(X1h, X1th, out X1s, out X1ts);

                Xs = X1s;
                Xts = X1ts;
            }
            else
            {
                Xs = Xh;
                Xts = Xth;
            }
        }

        public void ExpY(String Yh, Int32 Yth, out String Ys, out Int32 Yts)
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            String op = TokenManager.Instance.TokenSymbol;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["MULTIPLICACAO"])
            {
                op = "*";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["DIVISAO"])
            {
                op = "/";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MOD"] || opType == LexMap.Consts["MODC"])
            {
                op = "%";
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["DIV"] || opType == LexMap.Consts["DIVC"])
            {
                op = "/";
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                int cint = LexMap.Consts["INTEIRO"];
                int creal = LexMap.Consts["REAL"];


                String Kval;
                Int32 Ktipo;
                this.ExpK(out Kval, out Ktipo);

                String Y1h, Y1s;
                Int32 Y1th, Y1ts;
                
                Y1h = Yh + op + Kval;

                Y1th = 0;

                if(Yth == cint && Ktipo == cint)
                    Y1th = cint;

                if(Yth == creal)
                    Y1th = creal;

                if(Ktipo == creal)
                    Y1th = creal;

                if(Y1th == 0)
                    throw new AnalisadorException(
                            String.Format("Expressões incompatíveis(Y): {0}{1}{2}",
                                LexMap.TokenGetNome(Yth), op, LexMap.TokenGetNome(Ktipo)));

                this.ExpY(Y1h, Y1th, out Y1s, out Y1ts);

                Ys = Y1s;
                Yts = Y1ts;
            }
            else
            {
                Ys = Yh;
                Yts = Yth;
            }
        }

        public void ExpK(out String Kval, out Int32 Ktipo)
        {
            AnalisadorLexico.Analisar();

            string tk = TokenManager.Instance.TokenSymbol;
            int tkc = TokenManager.Instance.TokenCode;
            Kval = " ";
            Ktipo = 0;

            if (tkc == LexMap.Consts["CONSTINTEIRO"])
            {
                Kval += tk;
                Ktipo = LexMap.Consts["INTEIRO"];

                return;
            }

            if (tkc == LexMap.Consts["CONSTREAL"])
            {
                Kval += tk;
                Ktipo = LexMap.Consts["REAL"];

                return;
            }

            if (tkc == LexMap.Consts["STRING"])
            {
                Kval += tk;
                Ktipo = LexMap.Consts["STRING"];

                return;
            }

            if (tkc == LexMap.Consts["VERDADEIRO"])
            {
                Kval += "1";
                Ktipo = LexMap.Consts["LOGICO"];

                return;
            }

            if (tkc == LexMap.Consts["FALSO"])
            {
                Kval += "0";
                Ktipo = LexMap.Consts["LOGICO"];

                return;
            }

            if (tkc == LexMap.Consts["ID"])
            {
                TSymbol tsy = TS.Instance.GetSymbol(tk);
                if (tsy == null)
                {
                    throw new AnalisadorException(
                        String.Format("A variável ou função {0} não foi declarado.", tk));
                }

                if ((tsy.tipoEstrutura == TipoEstrutura.VariavelComum) && (tsy.Valor == null))
                {
                    throw new AnalisadorException(
                        String.Format("A variável {0} não foi iniciada.", tk));
                }

                if (tsy.tipoEstrutura == TipoEstrutura.Procedimento)
                {
                    throw new AnalisadorException(String.Format("O procedimento {0} não pode ser utilizado como função.", tk)); 
                }

                if (tsy.tipoEstrutura == TipoEstrutura.Funcao)
                {
                    AnalisadorLexico.Analisar();

                    if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
                    {
                        throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["ABREPAR"])));
                    }

                    String ParametrosChamadaProcFuncCod;
                    this.ParametrosChamadaProcFunc(tk, out ParametrosChamadaProcFuncCod);

                    if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                    {
                        throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"])));
                    }

                    Kval += ParametrosChamadaProcFuncCod;
                    Ktipo = tsy.tipo;
                }
                else
                {
                    Kval += tk;
                    Ktipo = tsy.tipo;
                }

                return;
            }

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ABREPAR"])
            {
                String ExpCod;
                Int32 ExpTipo;
                this.Exp(out ExpCod, out ExpTipo);
                Kval = " (" + ExpCod + " )";
                Ktipo = ExpTipo;

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException(
                       String.Format("Era esperado o token {0}", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"]))
                    );
                }

                return;
            }

            throw new AnalisadorException("O valor da expressão (K) não é válido");   
        }

        public void Atribuicao(String id, out String atribuicaoCod)
        {
            String ExpCod;
            Int32 ExpTipo;
            this.Exp(out ExpCod, out ExpTipo);

            TSymbol tsy = TS.Instance.GetSymbol(id);

            if (tsy == null)
            {
                throw new AnalisadorException(String.Format("Variável {0} não declarada para uma atribuição", id));
            }

            if ((tsy.tipoEstrutura == TipoEstrutura.Funcao) || (tsy.tipoEstrutura == TipoEstrutura.Procedimento))
            {
                throw new AnalisadorException(String.Format("Identificador {0} é um procedimento ou função e não pode receber atribuição.", tsy.id));
            }

            int cbool = LexMap.Consts["LOGICO"];
            int cint = LexMap.Consts["INTEIRO"];
            int creal = LexMap.Consts["REAL"];

            if (tsy.tipo == cbool && ExpTipo != cbool ||
                tsy.tipo == cint && ExpTipo == cbool ||
                tsy.tipo == creal && ExpTipo == cbool
                )
            {
                throw new AnalisadorException(
                            String.Format("Atribuição incompatível(Atrib): {0}{1}{2}",
                                LexMap.TokenGetNome(tsy.tipo), "<-", LexMap.TokenGetNome(ExpTipo)));
            }

            tsy.Valor = ExpCod;

            atribuicaoCod = id + " =" + ExpCod + ";" + Environment.NewLine;
        }

        #endregion expv2
        
        public bool ParEsc(out string ParEscStr, out string ParEscPar)
        {
            String EscCod;
            Int32 EscTipo;
            this.Exp(out EscCod, out EscTipo);

            int cbool = LexMap.Consts["LOGICO"];
            int cint = LexMap.Consts["INTEIRO"];
            int creal = LexMap.Consts["REAL"];
            int cstring = LexMap.Consts["STRING"];

            ParEscStr = "";
            ParEscPar = "";

            if (EscTipo == cbool || EscTipo == cint)
            {
                ParEscPar = EscCod;
                ParEscStr = "%d";

                return true;
            }

            if (EscTipo == creal)
            {
                ParEscPar = EscCod;
                ParEscStr = "%f";

                return true;
            }

            if (EscTipo == cstring)
            {
                ParEscPar = EscCod;
                ParEscStr = "%s";

                return true;
            }

            return false;
        }

        public void ListaParEsc(out string ListaParEscStr, out string ListaParEscPar)
        {
            ListaParEscStr = "";
            ListaParEscPar = "";

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
            {
                String ParEscStr, ParEscPar;
                bool f = this.ParEsc(out ParEscStr, out ParEscPar);
                if (!f)
                {
                    throw new AnalisadorException("Parâmetro inválido(ListaParEsc)");
                }

                String ListaParEscStr1 = "";
                String ListaParEscPar1 = "";

                this.ListaParEsc(out ListaParEscStr1, out ListaParEscPar1);

                ListaParEscStr = ParEscStr + ListaParEscStr1;
                ListaParEscPar = ", " + ParEscPar + ListaParEscPar1;
            }
        }
        
        public void Escreva(out String EECod)
        {
            String ParEscStr, ListaParEscStr, ParEscPar, ListaParEscPar;

            bool f = this.ParEsc(out ParEscStr, out ParEscPar);
            if (!f)
            {
                throw new AnalisadorException("Parâmetro inválido(Escreva(l))");
            }

            this.ListaParEsc(out ListaParEscStr, out ListaParEscPar);

            EECod = "printf(\"" + ParEscStr + ListaParEscStr + "\", " + ParEscPar + ListaParEscPar + ");" + Environment.NewLine;
        }

        public void Leia(out String LeiaCod)
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
            {
                throw new AnalisadorException("Leia necessita de uma variável");
            }

            string id = TokenManager.Instance.TokenSymbol;

            int cbool = LexMap.Consts["LOGICO"];
            int cint = LexMap.Consts["INTEIRO"];
            int creal = LexMap.Consts["REAL"];
            int cstring = LexMap.Consts["STRING"];

            TSymbol tsy = TS.Instance.GetSymbol(id);
            if (tsy == null)
	        {
		        throw new AnalisadorException(String.Format("Variável {0} não declarada em Leia", id));
	        }

            if ((tsy.tipoEstrutura == TipoEstrutura.Procedimento) || (tsy.tipoEstrutura == TipoEstrutura.Funcao))
            {
                throw new AnalisadorException("Comando Leia não pode ter um procedimento ou uma função.");
            }

            int idtipo = tsy.tipo;

            string por = "";
            bool rtype = false;

            if (idtipo == cbool || idtipo == cint)
            {
                por = "%d";
                rtype = true;
            }

            if (idtipo == creal)
            {
                por = "%f";
                rtype = true;
            }

            if (idtipo == cstring)
            {
                por = "%s";
                rtype = true;
            }

            LeiaCod = "scanf(\"" + por + "\", &" + id + ");" + Environment.NewLine;
            tsy.Valor = String.Empty;

            if (!rtype)
            {
                throw new AnalisadorException("Tipo inválido em Leia.");
            }

        }
        
        public void TipoComando(String id, out String tipoComandoCod)
        {
            var tcod = TokenManager.Instance.TokenCode;
            var tsym = TokenManager.Instance.TokenSymbol;

            tipoComandoCod = "";

            if (tcod == LexMap.Consts["RETORNE"])
            {
                String RetorneCod;
                this.Retorne(out RetorneCod);

                tipoComandoCod += RetorneCod;

                return;
            }
            else if (tcod == LexMap.Consts["SE"])
            {
                String SeCod;
                this.Se(out SeCod);

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FIMSE"])
                {
                    throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["FIMSE"])));
                }

                tipoComandoCod += SeCod;
                AnalisadorLexico.Analisar();

                return;
            }
            else if (tcod == LexMap.Consts["REPITA"])
            {
                String RepitaCod;
                this.Repita(out RepitaCod);

                tipoComandoCod += RepitaCod;

                return;
            }
            else if (tcod == LexMap.Consts["ENQUANTO"])
            {
                String EnquantoCod;
                this.Enquanto(out EnquantoCod);

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FIMENQUANTO"])
                {
                    throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["FIMENQUANTO"])));
                }

                tipoComandoCod += EnquantoCod;
                AnalisadorLexico.Analisar();

                return; 
            }
            else if (tcod == LexMap.Consts["PARA"])
            {
                String ParaCod;
                this.Para(out ParaCod);

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FIMPARA"])
                {
                    throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["FIMPARA"])));
                }

                tipoComandoCod += ParaCod;
                AnalisadorLexico.Analisar();

                return;
            }
            else
            {
                AnalisadorLexico.Analisar();

                if (TokenManager.Instance.TokenCode == LexMap.Consts["ATRIBUICAO"])
                {
                    String AtribuicaoCod;
                    this.Atribuicao(id, out AtribuicaoCod);

                    tipoComandoCod += AtribuicaoCod;

                    return;
                }

                if (TokenManager.Instance.TokenCode == LexMap.Consts["ABREPAR"])
                {
                    if (tcod == LexMap.Consts["ESCREVA"])
                    {
                        String ECod;
                        this.Escreva(out ECod);

                        if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                        {
                            throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"])));
                        }

                        tipoComandoCod += ECod;
                        AnalisadorLexico.Analisar();

                        return;
                    }

                    if (tcod == LexMap.Consts["LEIA"])
                    {
                        String LeiaCod;
                        this.Leia(out LeiaCod);

                        AnalisadorLexico.Analisar();
                        if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                        {
                            throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"])));
                        }

                        tipoComandoCod += LeiaCod;
                        AnalisadorLexico.Analisar();

                        return;
                    }

                    TSymbol symbolProxFunc = TS.Instance.GetSymbol(id);

                    if (symbolProxFunc != null)
                    {
                        if (symbolProxFunc.tipoEstrutura == TipoEstrutura.Procedimento)
                        {
                            String ParametrosChamadaProcFuncCod;
                            this.ParametrosChamadaProcFunc(symbolProxFunc.id, out ParametrosChamadaProcFuncCod);

                            if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                            {
                                throw new AnalisadorException(String.Format("Era esperado um {0}.", LexMap.TokenGetNome(LexMap.Consts["FECHAPAR"])));
                            }

                            tipoComandoCod += ParametrosChamadaProcFuncCod + ";" + Environment.NewLine;
                            AnalisadorLexico.Analisar();

                            return;
                        }
                        else if (symbolProxFunc.tipoEstrutura == TipoEstrutura.Funcao)
                        {
                            throw new AnalisadorException(String.Format("Função {0} não pode ser utilizado como um procedimento.", tsym));
                        }
                    }

                    throw new AnalisadorException(String.Format("Comando {0} não reconhecido", tsym));
                }
            }

            throw new AnalisadorException(String.Format("Caracter {0} inválido no contexto.", TokenManager.Instance.TokenSymbol));
        }

        private void ParametrosChamadaProcFunc(string id, out string ParametrosChamadaProcFuncCod)
        {
            string ParChamadaProcFuncCod = String.Empty;
            string ListaParChamadaProcFuncCod = String.Empty;

            List<TSymbol> listParameters = TS.Instance.GetChildSymbols(id);

            if(listParameters.Count > 0)
            {
                this.ParChamadaProcFunc(out ParChamadaProcFuncCod, listParameters[0]);

                if(listParameters.Count > 1)
                {
                    int index = 1;
                    this.ListaParChamadaProcFunc(id, out ListaParChamadaProcFuncCod, listParameters, ref index);

                    if(index < listParameters.Count)
                    {
                        throw new AnalisadorException(String.Format("Chamada do procedimento ou função {0} possui menos parametros do que a assinatura.", id));
                    }
                    else if ((index - 1) > (listParameters.Count - 1))
                    {
                        throw new AnalisadorException(String.Format("Chamada do procedimento ou função {0} possui mais parametros do que a assinatura.", id));
                    }
                }
            }
            else
            {
                AnalisadorLexico.Analisar();
            }

            ParametrosChamadaProcFuncCod = String.Format("{0}({1}{2})", id, ParChamadaProcFuncCod, ListaParChamadaProcFuncCod);
        }

        private void ListaParChamadaProcFunc(string id, out string ListaParPar, List<TSymbol> listaDeParametrosNecessaria, ref int index)
        {
            ListaParPar = String.Empty;

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
            {
                TSymbol symbol = null;

                try
                {
                    symbol = listaDeParametrosNecessaria[index];
                }
                catch(Exception)
                {
                    throw new AnalisadorException(String.Format("Chamada do procedimento ou função {0} possui mais parametros do que a assinatura.", id));
                }

                String ParPar;
                this.ParChamadaProcFunc(out ParPar, symbol);

                index++;
                string ListaParPar1 = String.Empty;
                this.ListaParChamadaProcFunc(id, out ListaParPar1, listaDeParametrosNecessaria, ref index);

                ListaParPar = ", " + ParPar + ListaParPar1;
            }
        }

        private void ParChamadaProcFunc(out string ParChamadaProcFuncCod, TSymbol parametroCorrente)
        {
            String Cod;
            Int32 Tipo;
            this.Exp(out Cod, out Tipo);

            if (parametroCorrente.tipoEstrutura == TipoEstrutura.VariavelParametroRef)
            {
                TSymbol symbolId = TS.Instance.GetSymbol(Cod.Trim());

                if (symbolId != null) //é um id único e válido
                {
                    ParChamadaProcFuncCod = String.Concat("&", Cod.Trim());
                }
                else //é uma expressão complexa, não pode ser parametro de ref, gera erro
                {
                    throw new AnalisadorException(String.Format("Parametro {0} inválido. Parametros de referencia são permitidos apenas para identificadores.", Cod));
                }
            }
            else
            {
                ParChamadaProcFuncCod = Cod;
            }

            if (parametroCorrente.tipo != Tipo)
            {
                throw new AnalisadorException(String.Format("Tipo do parametro {0} é inválido. Tipo incompatível do parametro entre chamada e assinatura.", Cod));
            }
        }

        private void Para(out string ParaCod)
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
            {
                throw new AnalisadorException("Comando Para necessita de uma variável.");
            }

            TSymbol tsy = TS.Instance.GetSymbol(TokenManager.Instance.TokenSymbol);

            if ((tsy.tipoEstrutura == TipoEstrutura.Procedimento) || (tsy.tipoEstrutura == TipoEstrutura.Funcao))
            {
                throw new AnalisadorException("Comando Para não pode ter um procedimento ou uma função.");
            }

            if (tsy.tipo != LexMap.Consts["INTEIRO"])
            {
                throw new AnalisadorException("Comando Para necessita de uma variável do tipo inteiro.");
            }

            tsy.Valor = String.Empty;
            String identifier = TokenManager.Instance.TokenSymbol;

            AnalisadorLexico.Analisar();

            String ExpCod;
            Int32 ExpTipo;
            this.Exp(out ExpCod, out ExpTipo);

            if (ExpTipo != LexMap.Consts["INTEIRO"])
            {
                throw new AnalisadorException(String.Format("Expressão final da primeira expressão não é do tipo inteiro. Tipo: {0}", LexMap.TokenGetNome(ExpTipo)));
            }

            if (TokenManager.Instance.TokenCode != LexMap.Consts["ATE"])
            {
                throw new AnalisadorException(String.Format("Esperado o token {0}.", LexMap.TokenGetNome(LexMap.Consts["ATE"])));
            }

            String ExpCod2;
            Int32 ExpTipo2;
            this.Exp(out ExpCod2, out ExpTipo2);

            if (ExpTipo2 != LexMap.Consts["INTEIRO"])
            {
                throw new AnalisadorException(String.Format("Expressão final da segunda expressão não é do tipo inteiro. Tipo: {0}", LexMap.TokenGetNome(ExpTipo)));
            }

            if (TokenManager.Instance.TokenCode != LexMap.Consts["FACA"])
            {
                throw new AnalisadorException(String.Format("Esperado o token {0}.", LexMap.TokenGetNome(LexMap.Consts["FACA"])));
            }

            String ListaDeComandosCod;
            this.ListaDeComandos(out ListaDeComandosCod);

            ParaCod = "for (" + identifier + " =" + ExpCod + "; " + identifier + " <=" + ExpCod2 + "; " + identifier + "++)" + Environment.NewLine + IndentationManager.Instance.GetIndentation() + "{";
            ParaCod += Environment.NewLine + ListaDeComandosCod;
            ParaCod += IndentationManager.Instance.GetIndentation() + "}" + Environment.NewLine;
        }

        private void Enquanto(out string EnquantoCod)
        {
            String ExpCod;
            Int32 ExpTipo;
            this.Exp(out ExpCod, out ExpTipo);
            
            if (ExpTipo == LexMap.Consts["LOGICO"])
            {
                if (TokenManager.Instance.TokenCode != LexMap.Consts["FACA"])
                {
                    throw new AnalisadorException(String.Format("Esperado o token {0}.", LexMap.TokenGetNome(LexMap.Consts["FACA"])));
                }

                String ListaDeComandosCod;
                this.ListaDeComandos(out ListaDeComandosCod);

                EnquantoCod = "while (" + ExpCod + ") " + Environment.NewLine + IndentationManager.Instance.GetIndentation() + "{";
                EnquantoCod += Environment.NewLine + ListaDeComandosCod;
                EnquantoCod += IndentationManager.Instance.GetIndentation() + "}" + Environment.NewLine;
            }
            else
            {
                throw new AnalisadorException(String.Format("Expressão final não é do tipo lógico. Tipo: {0}", LexMap.TokenGetNome(ExpTipo)));
            }
        }

        private void Repita(out string RepitaCod)
        {
            String ListaDeComandosCod;
            this.ListaDeComandos(out ListaDeComandosCod);

            RepitaCod = "do "+ Environment.NewLine + IndentationManager.Instance.GetIndentation() + "{";
            RepitaCod += Environment.NewLine + ListaDeComandosCod;

            if (TokenManager.Instance.TokenCode != LexMap.Consts["ATE"])
            {
                throw new AnalisadorException(String.Format("Esperado o token {0}.", LexMap.TokenGetNome(LexMap.Consts["ATE"])));
            }

            String ExpCod;
            Int32 ExpTipo;
            this.Exp(out ExpCod, out ExpTipo);

            if (ExpTipo == LexMap.Consts["LOGICO"])
            {
                RepitaCod += IndentationManager.Instance.GetIndentation() + "} while (" + ExpCod + ");" + Environment.NewLine;
            }
            else
            {
                throw new AnalisadorException(String.Format("Expressão final não é do tipo lógico. Tipo: {0}", LexMap.TokenGetNome(ExpTipo)));
            }
        }

        private void Retorne(out string RetorneCod)
        {
            String ExpCod;
            Int32 ExpTipo;
            this.Exp(out ExpCod, out ExpTipo);

            if (!TS.CurrentContext.Equals(TS.MainContext))
            {
                TSymbol function = TS.Instance.GetSymbol(TS.CurrentContext);

                if (function != null)
                {
                    if (function.tipo == LexMap.Consts["VAZIO"])
                    {
                        throw new AnalisadorException(String.Format("Procedimento {0} não deve retornar valor.", function.id));
                    }

                    if (ExpTipo != function.tipo)
                    {
                        throw new AnalisadorException(String.Format("Tipo de retorno incompatível para a função {0} .", function.id));
                    }

                    RetorneCod = String.Concat("return " + ExpCod + Environment.NewLine);
                }
                else
                {
                    throw new AnalisadorException(String.Format("Função {0} não encontrada.", TS.CurrentContext));
                }
            }
            else
            {
                throw new AnalisadorException("Comando retorne deve estar no escopo de uma função.");
            }
        }

        private void Se(out string SeCod)
        {
            String ExpCod;
            Int32 ExpTipo;
            this.Exp(out ExpCod, out ExpTipo);

            if (ExpTipo == LexMap.Consts["LOGICO"])
            {
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ENTAO"])
                {
                    throw new AnalisadorException(String.Format("Esperado o token {0}.", LexMap.TokenGetNome(LexMap.Consts["ENTAO"])));
                }

                String ListaDeComandosCod;
                this.ListaDeComandos(out ListaDeComandosCod);

                SeCod = "if (" + ExpCod + ") " + Environment.NewLine + IndentationManager.Instance.GetIndentation() + "{";
                SeCod += Environment.NewLine + ListaDeComandosCod;

                if (TokenManager.Instance.TokenCode == LexMap.Consts["SENAO"])
                {
                    SeCod += IndentationManager.Instance.GetIndentation() + "}" + Environment.NewLine + IndentationManager.Instance.GetIndentation() + "else" + Environment.NewLine + IndentationManager.Instance.GetIndentation() + "{";

                    ListaDeComandosCod = String.Empty;
                    this.ListaDeComandos(out ListaDeComandosCod);

                    SeCod += Environment.NewLine + ListaDeComandosCod;
                }

                SeCod += IndentationManager.Instance.GetIndentation() + "}" + Environment.NewLine;
            }
            else
            {
                throw new AnalisadorException(String.Format("Expressão final não é do tipo lógico. Tipo: {0}", LexMap.TokenGetNome(ExpTipo)));
            }
        }

        public void ListaDeComandos(out String listaDeComandosCod)
        {
            AnalisadorLexico.Analisar();
            string id = TokenManager.Instance.TokenSymbol;
            listaDeComandosCod = "";
            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["ESCREVA"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["LEIA"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["SE"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["PARA"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["ENQUANTO"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["REPITA"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["RETORNE"]
                )
            {
                IndentationManager.Instance.Increase();

                string TipoComandoCod;
                this.TipoComando(id, out TipoComandoCod);
                listaDeComandosCod = IndentationManager.Instance.GetIndentation() + TipoComandoCod;

                IndentationManager.Instance.Decrement();

                string dtk = TokenManager.Instance.TokenSymbol;
                LineManager.Instance.ResetToLastPos();

                String listaDeComandos1Cod = "";
                this.ListaDeComandos(out listaDeComandos1Cod);
                listaDeComandosCod += listaDeComandos1Cod;
            }
        }

        private void Algoritmo(out String algCod)
        {
            algCod = String.Empty;

            String bibliotecaCod;
            this.Bibliotecas(out bibliotecaCod);

            String nomeAlgoritmoCod;
            this.NomeDoAlgoritmo(out nomeAlgoritmoCod);

            TS.MainContext = nomeAlgoritmoCod.Trim();
            TS.CurrentContext = TS.MainContext;

            String declaracoesCod = String.Empty;

            if (this.Declaracoes(out declaracoesCod))
            {
                algCod = bibliotecaCod + Environment.NewLine;
                algCod += declaracoesCod + Environment.NewLine;

                if (TokenManager.Instance.TokenCode != LexMap.Consts["INICIO"])
                {
                    throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["INICIO"]));
                }

                String ListaDeComandosCod;
                this.ListaDeComandos(out ListaDeComandosCod);

                algCod += "void " + nomeAlgoritmoCod + "()" + Environment.NewLine + "{";
                algCod += Environment.NewLine + ListaDeComandosCod;
                algCod += "}" + Environment.NewLine;

                algCod += "int main()" + Environment.NewLine + "{" + Environment.NewLine + "\t" + nomeAlgoritmoCod + "();" + Environment.NewLine + "}" + Environment.NewLine;

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FIMALGORITMO"])
                {
                    throw new AnalisadorException("Esperado o token " + LexMap.TokenGetNome(LexMap.Consts["FIMALGORITMO"]));
                }
            }
        }
    }
}