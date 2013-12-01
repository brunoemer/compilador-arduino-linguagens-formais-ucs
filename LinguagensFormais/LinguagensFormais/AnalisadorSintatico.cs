using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinguagensFormais
{
    public class AnalisadorSintatico
    {
        public String Execute()
        {
            /*
            if (LineManager.Instance.ReadLine())
            {
                while (true)
                {
                    try
                    {
                        AnalisadorLexico.Analisar();
                    }
                    catch (AnalisadorFimArquivoException)
                    {
                        return "";
                    }
                }
            }
            */
            if (LineManager.Instance.ReadLine())
            {
                try
                {
                    this.ProgArduino();
                }
                catch (AnalisadorFimArquivoException)
                {
                    return "";
                }
            }

            return "";
        }

        private void ProgArduino()
        {
            this.ListaProg();
        }

        private void ListaProg()
        {
            this.DeclaraFuncao();

            this.ListaComandos();

            this.ListaProg();
        }

        private void ListaComandos()
        {
            AnalisadorLexico.Analisar();

            //declaração variaveis
            if (TokenManager.Instance.TokenCode == LexMap.Consts["INTEIRO"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["FLOAT"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["BYTE"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["LONG"])
            {
                LineManager.Instance.ResetToLastPos();
                this.DeclaraVar();
            }

            //if
            if (TokenManager.Instance.TokenCode == LexMap.Consts["IF"])
            {
                this.If();
            }

            //while
            if (TokenManager.Instance.TokenCode == LexMap.Consts["WHILE"])
            {
                this.While();
            }

            //do while
            if (TokenManager.Instance.TokenCode == LexMap.Consts["DO"])
            {
                this.DoWhile();
            }

            //for
            if (TokenManager.Instance.TokenCode == LexMap.Consts["FOR"])
            {
                this.For();
            }

            //switch
            if (TokenManager.Instance.TokenCode == LexMap.Consts["SWITCH"])
            {
                this.Switch();
            }
            //switch break
            if (TokenManager.Instance.TokenCode == LexMap.Consts["BREAK"])
            {
                return;
            }

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
            {
                //atribuição
                this.Atribuicao();

                //funcao
                if (TokenManager.Instance.TokenCode == LexMap.Consts["ABREPAR"])
                {
                    //nao retrocedeu????
                    LineManager.Instance.ResetToLastPos();
                    this.Funcao();
                }

                if (TokenManager.Instance.TokenCode != LexMap.Consts["PONTOVIRGULA"])
                {
                    throw new AnalisadorException("O Token ; era esperado");
                }
            }

            //verifica se é declaração de funcao, sai da lista de comandos
            try
            {
                LineManager.Instance.ResetToLastPos();
                this.TipoRetorno();
                LineManager.Instance.ResetToLastPos();
                return;
            }
            catch (AnalisadorException exc) { }

            // fim de bloco
            if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
            {
                //recursão
                try
                {
                    this.ListaComandos();
                }
                catch (AnalisadorFimArquivoException exc) { }
            }
        }

        private void DeclaraVar()
        {
            this.TipoVar();

            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
            {
                throw new AnalisadorException("Um identificador era esperado.");
            }

            this.ListaVar();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["PONTOVIRGULA"])
            {
                throw new AnalisadorException("O token ; era esperado.");
            }
        }

        public void TipoVar()
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["INTEIRO"] &&
                TokenManager.Instance.TokenCode != LexMap.Consts["FLOAT"] &&
                TokenManager.Instance.TokenCode != LexMap.Consts["BYTE"] &&
                TokenManager.Instance.TokenCode != LexMap.Consts["LONG"])
            {
                throw new AnalisadorException("Tipo de variável não pode ser identificado.");
            }
        }

        public void ListaVar()
        {
            AnalisadorLexico.Analisar();

            if (TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
            {
                AnalisadorLexico.Analisar();

                if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
                {
                    this.ListaVar();
                }
                else
                {
                    throw new AnalisadorException("Tipo de variável não pode ser identificado.");
                }
            }
        }

        public void Exp()
        {
            this.ExpT();

            this.ExpR();
        }

        public void ExpR()
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["OU"])
            {
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                this.ExpT();

                this.ExpR();
            }
        }

        public void ExpT()
        {
            this.ExpF();

            this.ExpU();
            LineManager.Instance.ResetToLastPos();
        }

        public void ExpF()
        {
            this.ExpG();
        }

        public void ExpU()
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["E"])
            {
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                this.ExpF();

                this.ExpU();
            }
        }

        public void ExpG()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode == LexMap.Consts["NAO"])
            {
                this.ExpG();
            }
            else
            {
                //ACHO
                LineManager.Instance.ResetToLastPos();

                this.ExpH();


                this.ExpV();

                LineManager.Instance.ResetToLastPos();
            }
        }

        public void ExpV()
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["MAIOR"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["IGUAL"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MENOR"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MAIORIGUAL"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["DIFERENTE"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MENORIGUAL"])
            {
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                this.ExpH();

                this.ExpV();
            }
        }

        public void ExpH()
        {
            this.ExpJ();

            this.ExpX();
            LineManager.Instance.ResetToLastPos();
        }

        public void ExpJ()
        {
            this.ExpK();

            this.ExpY();
            LineManager.Instance.ResetToLastPos();
        }

        public void ExpX()
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["MAIS"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MENOS"])
            {
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                this.ExpJ();

                this.ExpX();
            }
        }

        public void ExpY()
        {
            AnalisadorLexico.Analisar();
            Int32 opType = TokenManager.Instance.TokenCode;
            bool is_valid_flag = false;

            if (opType == LexMap.Consts["MULTIPLICACAO"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["DIVISAO"])
            {
                is_valid_flag = true;
            }

            if (opType == LexMap.Consts["MODC"])
            {
                is_valid_flag = true;
            }

            if (is_valid_flag)
            {
                this.ExpK();

                this.ExpY();
            }
        }

        public void ExpK()
        {
            AnalisadorLexico.Analisar();
            int tkc = TokenManager.Instance.TokenCode;

            if (tkc == LexMap.Consts["CONSTINTEIRO"] ||
                tkc == LexMap.Consts["CONSTFLOAT"] ||
                tkc == LexMap.Consts["CONSTFLOATPONTO"] ||
                tkc == LexMap.Consts["CONSTFLOATPONTONUM"] ||
                tkc == LexMap.Consts["CONSTFLOATNUME"] ||
                tkc == LexMap.Consts["CONSTFLOATNUMPONTO"] ||
                tkc == LexMap.Consts["CONSTFLOATE"] ||
                tkc == LexMap.Consts["TRUE"] ||
                tkc == LexMap.Consts["FALSE"] ||
                tkc == LexMap.Consts["HIGH"] ||
                tkc == LexMap.Consts["LOW"] ||
                tkc == LexMap.Consts["INPUT"] ||
                tkc == LexMap.Consts["OUTPUT"] ||
                tkc == LexMap.Consts["ID"])
            {
                return;
            }

            if (TokenManager.Instance.TokenCode == LexMap.Consts["ABREPAR"])
            {
                this.Exp();

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

        public void Atribuicao()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode == LexMap.Consts["ATRIBUICAO"])
            {
                this.Exp();
            }

        }

        private void If()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["IF"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
                {
                    throw new AnalisadorException("O token ( era esperado");
                }

                this.Exp();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException("O token ) era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABRECHAVES"])
                {
                    throw new AnalisadorException("O token { era esperado");
                }

                this.ListaComandos();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
                {
                    throw new AnalisadorException("O token } era esperado");
                }

                this.IfEnd();
            }
        }

        private void IfEnd()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode == LexMap.Consts["ELSE"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode == LexMap.Consts["ABRECHAVES"])
                {
                    LineManager.Instance.ResetToLastPos();
                    this.ListaComandos();

                    if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
                    {
                        throw new AnalisadorException("O token } era esperado");
                    }
                }
                else
                {
                    this.If();
                    //loop?
                }
            }
            else
            {
                //retrocede?
                //LineManager.Instance.ResetToLastPos();
            }
        }

        private void While()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["WHILE"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
                {
                    throw new AnalisadorException("O token ( era esperado");
                }

                this.Exp();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException("O token ) era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABRECHAVES"])
                {
                    throw new AnalisadorException("O token { era esperado");
                }

                this.ListaComandos();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
                {
                    throw new AnalisadorException("O token } era esperado");
                }
            }
        }

        private void DoWhile()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["DO"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABRECHAVES"])
                {
                    throw new AnalisadorException("O token { era esperado");
                }

                this.ListaComandos();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
                {
                    throw new AnalisadorException("O token } era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["WHILE"])
                {
                    throw new AnalisadorException("O token while era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
                {
                    throw new AnalisadorException("O token ( era esperado");
                }

                this.Exp();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException("O token ) era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["PONTOVIRGULA"])
                {
                    throw new AnalisadorException("O token ; era esperado");
                }
            }
        }

        private void For()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["FOR"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
                {
                    throw new AnalisadorException("O token ( era esperado");
                }

                this.ListaAtrib();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["PONTOVIRGULA"])
                {
                    throw new AnalisadorException("O token ; era esperado");
                }

                this.Exp();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["PONTOVIRGULA"])
                {
                    throw new AnalisadorException("O token ; era esperado");
                }

                // falta o i++
                this.ListaAtrib();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException("O token ) era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABRECHAVES"])
                {
                    throw new AnalisadorException("O token { era esperado");
                }

                this.ListaComandos();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
                {
                    throw new AnalisadorException("O token } era esperado");
                }
            }
        }

        private void ListaAtrib()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
            {
                throw new AnalisadorException("O token identificador era esperado");
            }
            this.Atribuicao();

            this.ListaAtribA();
        }

        private void ListaAtribA()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
                {
                    throw new AnalisadorException("O token identificador era esperado");
                }

                this.Atribuicao();

                this.ListaAtribA();

                // reducao?
                //this.ListaAtrib();

            }
        }

        private void Switch()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["SWITCH"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
                {
                    throw new AnalisadorException("O token ( era esperado");
                }

                this.Exp();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException("O token ) era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABRECHAVES"])
                {
                    throw new AnalisadorException("O token { era esperado");
                }

                this.ListaCase();

                this.SwitchDefault();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
                {
                    throw new AnalisadorException("O token } era esperado");
                }
            }
        }

        private void ListaCase()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode == LexMap.Consts["CASE"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"] && TokenManager.Instance.TokenCode != LexMap.Consts["CONSTINTEIRO"])
                {
                    throw new AnalisadorException("O token identificador era esperado");
                }

                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["DOISPONTOS"])
                {
                    throw new AnalisadorException("O token : era esperado");
                }

                // retorna somente quando encontra }, feito gambiara para retornar quando tem break, quando nao tem ta com problema
                // ??????
                this.ListaComandos();

                this.CaseEnd();

                this.ListaCase();
            }

        }

        private void CaseEnd()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["BREAK"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["PONTOVIRGULA"])
                {
                    throw new AnalisadorException("O token ; era esperado");
                }

            }
        }

        private void SwitchDefault()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["DEFAULT"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["DOISPONTOS"])
                {
                    throw new AnalisadorException("O token : era esperado");
                }

                this.ListaComandos();

                this.CaseEnd();

            }
        }

        private void DeclaraFuncao()
        {
            this.TipoRetorno();

            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
            {
                throw new AnalisadorException("O token identificador era esperado");
            }

            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
            {
                //verifica se é declaração de variável, sai da DeclaraFuncao
                if (TokenManager.Instance.TokenCode == LexMap.Consts["PONTOVIRGULA"] || TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
                {
                    LineManager.Instance.ResetToLastPos();
                    LineManager.Instance.ResetToLastPos();
                    LineManager.Instance.ResetToLastPos();
                    return;
                }

                throw new AnalisadorException("O token ( era esperado");
            }

            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
            {
                LineManager.Instance.ResetToLastPos();
                this.ListaDecParam();
            }

            if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
            {
                throw new AnalisadorException("O token ) era esperado");
            }

            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["ABRECHAVES"])
            {
                throw new AnalisadorException("O token { era esperado");
            }

            this.ListaComandos();

            this.Retorno();

            if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHACHAVES"])
            {
                throw new AnalisadorException("O token } era esperado");
            }
        }

        private void TipoRetorno()
        {
            try
            {
                this.TipoVar();
            } 
            catch (AnalisadorException exc)
            {
                if (TokenManager.Instance.TokenCode != LexMap.Consts["VOID"])
                {
                    throw new AnalisadorException("Tipo de variável de função não pode ser identificado");
                }
            }
        }

        private void ListaDecParam()
        {
            this.TipoVar();

            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
            {
                throw new AnalisadorException("O token identificador era esperado");
            }

            this.ListaDecParamA();
        }

        private void ListaDecParamA()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
            {
                this.ListaDecParam();
            }
        }

        private void Retorno()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["RETURN"])
            {
                // opcional?
                this.Exp();

                if (TokenManager.Instance.TokenCode != LexMap.Consts["PONTOVIRGULA"])
                {
                    throw new AnalisadorException("O token ; era esperado");
                }
            }
        }

        private void Funcao()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
            {
                AnalisadorLexico.Analisar();
                if (TokenManager.Instance.TokenCode != LexMap.Consts["ABREPAR"])
                {
                    throw new AnalisadorException("O token ( era esperado");
                }

                AnalisadorLexico.Analisar();
                //se for sem parametro
                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    LineManager.Instance.ResetToLastPos();
                    this.ListaParam();
                }

                if (TokenManager.Instance.TokenCode != LexMap.Consts["FECHAPAR"])
                {
                    throw new AnalisadorException("O token ) era esperado");
                }
            }
        }

        private void ListaParam()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode != LexMap.Consts["ID"])
            {
                this.Exp();
            }
            else
            {
                AnalisadorLexico.Analisar();
            }

            this.ListaParamRec();
        }

        private void ListaParamRec()
        {
            if (TokenManager.Instance.TokenCode == LexMap.Consts["VIRGULA"])
            {
                this.ListaParam();
            }
        }
    }
}