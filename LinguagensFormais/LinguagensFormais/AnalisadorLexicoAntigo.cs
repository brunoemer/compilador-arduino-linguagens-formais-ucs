using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinguagensFormais
{
    public class AnalisadorLexicoAntigo
    {
        public static void Analisar()
        {
            String nome;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("Token: {0}", TokenManager.Instance.TokenSymbol));
            sb.AppendLine(String.Format("Token Anterior: {0}", TokenManager.Instance.TokenSymbolAnt));
            sb.AppendLine(String.Format("Código do Token: {0}", TokenManager.Instance.TokenCode));
            sb.AppendLine(String.Format("Código do Token Anterior: {0}", TokenManager.Instance.TokenCodeAnt));
            sb.AppendLine(String.Format("Posição do Caracter de parada: {0}", LineManager.Instance.PosCurrentCaracter));
            sb.AppendLine(String.Format("Posição de início de leitura do Token: {0}", LineManager.Instance.PosStartToken));

            if (AnalisadorLexico.Reconhecedor())
            {
                if (TokenManager.Instance.TokenCode == LexMap.Consts["ID"])
                {
                    try
                    {
                        TokenManager.Instance.TokenCode = LexMap.PalavraReservada[TokenManager.Instance.TokenSymbol];
                    }
                    catch (Exception) { }
                }

                try
                {
                    nome = LexMap.TokenNome[TokenManager.Instance.TokenCode];

                    LineManager.Instance.LinesOut.Add(
                        String.Format("Cod: {0}\tLinha: {1}\tTipo: {2}\tLexema: {3}\n",
                        TokenManager.Instance.TokenCode, 
                        LineManager.Instance.LineIndex, 
                        nome,
                        TokenManager.Instance.TokenSymbol));
                }
                catch (Exception)
                {
                    throw new AnalisadorException(String.Format("O Token -{0}- não foi encontrado", TokenManager.Instance.TokenSymbol));
                }
            }
            else
            {
                throw new Exception("O reconhecedor encontrou uma sintaxe não finalizada" + Environment.NewLine + sb.ToString());
            }
        }

        public static bool Reconhecedor()
        {
            Int32 estado = 0;
            TokenManager.Instance.TokenSymbol = String.Empty;
            TokenManager.Instance.TokenCode = 0;

            bool endofline = false;
            char c = '\0';

            //Busco próximo caracter (mesmo que tenha que pular linhas)
            if (LineManager.Instance.PosCurrentCaracter < LineManager.Instance.LineContent.Length)
            {
                c = LineManager.Instance.LineContent[LineManager.Instance.PosCurrentCaracter];
            }
            else
            {
                if (!LineManager.Instance.ReadLine())
                {
                    return false;
                }

                while (String.IsNullOrEmpty(LineManager.Instance.LineContent.Trim()))
                {
                    if (!LineManager.Instance.ReadLine())
                    {
                        return false;
                    }
                }
                c = LineManager.Instance.LineContent[LineManager.Instance.PosCurrentCaracter];
            }

            while (c == ' ' || c == '\n' || Environment.NewLine.Equals(c) || c == '\t')
            {
                LineManager.Instance.PosCurrentCaracter++;

                if (LineManager.Instance.PosCurrentCaracter >= LineManager.Instance.LineContent.Length)
                {
                    while (String.IsNullOrEmpty(LineManager.Instance.LineContent.Trim()))
                    {
                        if (!LineManager.Instance.ReadLine())
                        {
                            return false;
                        }
                    }
                }

                c = LineManager.Instance.LineContent[LineManager.Instance.PosCurrentCaracter];
            }
            //FIM -> Termino de buscar o próximo caracter

            while (endofline == false)
            {
                if (estado == 0)
                {
                    if (c == ' ' || c == '\n' || Environment.NewLine.Equals(c) || c == '\t')
                    {
                        //nada
                    }
                    else
                    {
                        estado = LexMap.Tipo(c);
                        if (estado <= 0)
                        {
                            return false;
                        }
                    }

                    if (c != ' ' && c != '\t')
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosStartToken = LineManager.Instance.PosCurrentCaracter;
                    }

                    LineManager.Instance.PosCurrentCaracter++;
                }


                endofline = false;
                if (LineManager.Instance.PosCurrentCaracter >= LineManager.Instance.LineContent.Length)
                {
                    endofline = true;
                }
                else
                {
                    c = LineManager.Instance.LineContent[LineManager.Instance.PosCurrentCaracter];
                }


                // Segunda "volta" concatena o token inteiro

                if (estado == LexMap.Consts["CONSTINTEIRO"])
                {
                    if (LexMap.Numeros.Contains(c) && endofline == false)
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                    }
                    else if(c.Equals('.') && endofline == false)
                    {
                        estado = LexMap.Consts["CONSTREAL"];
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                    }
                    else
                    {
                        TokenManager.Instance.TokenCode = LexMap.Consts["CONSTINTEIRO"];
                        return true;
                    }
                }
                else if (estado == LexMap.Consts["CONSTREAL"])
                {
                    if (LexMap.Numeros.Contains(c) && endofline == false)
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                    }
                    else
                    {
                        TokenManager.Instance.TokenCode = LexMap.Consts["CONSTREAL"];
                        return true;
                    }
                }
                else if (estado == LexMap.Consts["ID"])
                {
                    if ((LexMap.Letras.Contains(c) || 
                         LexMap.Numeros.Contains(c) || 
                         LexMap.Caracteres.Contains(c)) && 
                         (endofline == false))
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                    }
                    else
                    {
                        TokenManager.Instance.TokenCode = LexMap.Consts["ID"];
                        return true;
                    }
                }
                else if (estado == LexMap.Consts["STRING"])
                {
                    if (endofline == true)
                    {
                        return false;
                    }

                    if (c != '"')
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                    }
                    else
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;

                        TokenManager.Instance.TokenCode = LexMap.Consts["STRING"];

                        return true;
                    }
                }
                else if (estado == LexMap.Consts["DOISPONTOS"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["DOISPONTOS"];

                    return true;
                }
                else if (estado == LexMap.Consts["VIRGULA"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["VIRGULA"];

                    return true;
                }
                else if (estado == LexMap.Consts["ABREPAR"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["ABREPAR"];

                    return true;
                }
                else if (estado == LexMap.Consts["FECHAPAR"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["FECHAPAR"];

                    return true;
                }
                else if (estado == LexMap.Consts["MAIS"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["MAIS"];

                    return true;
                }
                else if (estado == LexMap.Consts["MENOS"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["MENOS"];

                    return true;
                }
                else if (estado == LexMap.Consts["MULTIPLICACAO"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["MULTIPLICACAO"];

                    return true;
                }
                else if (estado == LexMap.Consts["DIVISAO"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["DIVISAO"];

                    return true;
                }
                else if (estado == LexMap.Consts["MENOR"])
                {
                    if (c.Equals('-') && endofline == false)
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                        TokenManager.Instance.TokenCode = LexMap.Consts["ATRIBUICAO"];
                    }
                    else if (c.Equals('=') && endofline == false)
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                        TokenManager.Instance.TokenCode = LexMap.Consts["MENORIGUAL"];
                    }
                    else if (c.Equals('>') && endofline == false)
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                        TokenManager.Instance.TokenCode = LexMap.Consts["DIFERENTE"];
                    }
                    else
                    {
                        TokenManager.Instance.TokenCode = LexMap.Consts["MENOR"];
                    }

                    return true;
                }
                else if (estado == LexMap.Consts["IGUAL"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["IGUAL"];

                    return true;
                }
                else if (estado == LexMap.Consts["MAIOR"])
                {
                    if (c.Equals('=') && endofline == false)
                    {
                        TokenManager.Instance.TokenSymbol += c;
                        LineManager.Instance.PosCurrentCaracter++;
                        TokenManager.Instance.TokenCode = LexMap.Consts["MAIORIGUAL"];
                    }
                    else
                    {
                        TokenManager.Instance.TokenCode = LexMap.Consts["MAIOR"];
                    }

                    return true;
                }
                else if (estado == LexMap.Consts["DIVC"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["DIVC"];

                    return true;
                }
                else if (estado == LexMap.Consts["MODC"])
                {
                    TokenManager.Instance.TokenCode = LexMap.Consts["MODC"];

                    return true;
                }
            }

            return false;
        }
    }
}
