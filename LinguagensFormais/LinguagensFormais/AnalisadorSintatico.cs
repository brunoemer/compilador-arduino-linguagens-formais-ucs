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
            this.ListaComandos();
        }

        private void ListaComandos()
        {
            AnalisadorLexico.Analisar();
            if (TokenManager.Instance.TokenCode == LexMap.Consts["INTEIRO"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["FLOAT"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["BYTE"] ||
                TokenManager.Instance.TokenCode == LexMap.Consts["LONG"])
            {
                LineManager.Instance.ResetToLastPos();
                this.DeclaraVar();
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
    }
}