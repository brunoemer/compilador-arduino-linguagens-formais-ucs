﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompiladoresTrabalho
{
    public class LexMap
    {
        public static Dictionary<String, Int32> Consts = new Dictionary<String, Int32>()
        {
            {"CONSTINTEIRO", 1},
            {"ID", 2},
            {"MAIS", 3},
            {"MENOS", 4},
            {"MULTIPLICACAO", 5},
            {"DIVISAO", 6},
            {"MOD", 7},
            {"MENOR", 8},
            {"MAIOR", 9},
            {"IGUAL", 10},
            {"MAIORIGUAL", 11},
            {"MENORIGUAL", 12},
            {"DIFERENTE", 13},
            {"ATRIBUICAO", 14},
            {"DIV", 15},
            {"ABREPAR", 16},
            {"FECHAPAR", 17},
            {"DOISPONTOS", 20},
            {"VIRGULA", 22},
            {"CONSTREAL", 25},
            {"ALGORITMO", 26},
            {"VAR", 27},
            {"INICIO", 28},
            {"FIMALGORITMO", 29},
            {"ESCREVA", 42},
            {"LEIA", 43},
            {"ESCREVAL", 44},
            {"INTEIRO", 45},
            {"REAL", 46},
            {"E", 47},
            {"OU", 48},
            {"LOGICO", 51},
            {"VERDADEIRO", 52},
            {"FALSO", 53},
            {"NAO", 59},
            {"STRING", 61},
            {"DIVC", 64},
            {"MODC", 65},
            {"SE", 66},
            {"ENTAO", 67},
            {"SENAO", 68},
            {"FIMSE", 69},
            {"PARA", 70},
            {"DE", 71},
            {"ATE", 72},
            {"FACA", 73},
            {"FIMPARA", 74},
            {"ENQUANTO", 75},
            {"FIMENQUANTO", 76},
            {"REPITA", 77},
            {"PROCEDIMENTO", 78},
            {"FIMPROCEDIMENTO", 79},
            {"VAZIO", 80},
            {"FUNCAO", 81},
            {"FIMFUNCAO", 82},
            {"RETORNE", 83}
        };

        public static Dictionary<String, Int32> PalavraReservada = new Dictionary<String, Int32>()
        {
            {"mod", 7},
            {"div", 15},
            {"algoritmo", 26},
            {"var", 27},
            {"inicio", 28},
            {"fimalgoritmo", 29},
            {"inteiro", 45},
            {"escreva", 42},
            {"leia", 43},
            {"escreval", 44},
            {"real", 46},
            {"e", 47},
            {"ou", 48},
            {"verdadeiro", 52},
            {"falso", 53},
            {"nao", 59},
            {"logico", 51},
            {"se", 66},
            {"entao", 67},
            {"senao", 68},
            {"fimse", 69},
            {"para", 70},
            {"de", 71},
            {"ate", 72},
            {"faca", 73},
            {"fimpara", 74},
            {"enquanto", 75},
            {"fimenquanto", 76},
            {"repita", 77},
            {"procedimento", 78},
            {"fimprocedimento", 79},
            {"vazio", 80},
            {"funcao", 81},
            {"fimfuncao", 82},
            {"retorne", 83}
        };

        public static Dictionary<Int32, String> TokenNome = new Dictionary<Int32, String>()
        {
            {1, "Tk_Const_Int"},
            {2, "Tk_Ident"},
            {3, "Tk_Mais"},
            {4, "Tk_Menos"},
            {5, "Tk_Multiplicacao"},
            {6, "Tk_Divisao"},
            {7, "Tk_Resto"},
            {8, "Tk_Menor"},
            {9, "Tk_Maior"},
            {10, "Tk_Igual"},
            {11, "Tk_Maior_Igual"},
            {12, "Tk_Menor_Igual"},
            {13, "Tk_Diferente"},
            {14, "Tk_Atribuicao"},
            {15, "Tk_Div_Inteira"},
            {16, "Tk_Abre_Parenteses"},
            {17, "Tk_Fecha_Parenteses"},
            {20, "Tk_Dois_Pontos"},
            {22, "TK_Virgula"},
            {25, "Tk_Const_Real"},
            {26, "Tk_Algoritmo"},
            {27, "Tk_Var"},
            {28, "Tk_Inicio"},
            {29, "Tk_Fim_Algoritmo"},
            {42, "Tk_Escreva"},
            {43, "Tk_Leia"},
            {44, "Tk_Escreval"},
            {45, "Tk_Inteiro"},
            {46, "Tk_Real"},
            {47, "Tk_E"},
            {48, "Tk_Ou"},
            {51, "Tk_Logico"},
            {52, "Tk_Verdadeiro"},
            {53, "Tk_Falso"},
            {59, "Tk_Nao"},
            {61, "Tk_String"},
            {64, "Tk_Div_Inteira_Char"},
            {65, "Tk_Resto_Char"},
            {66, "Tk_Se"},
            {67, "Tk_Entao"},
            {68, "Tk_SeNao"},
            {69, "Tk_FimSe"},
            {70, "Tk_Para"},
            {71, "Tk_De"},
            {72, "Tk_Ate"},
            {73, "Tk_Faca"},
            {74, "Tk_FimPara"},
            {75, "Tk_Enquanto"},
            {76, "Tk_FimEnquanto"},
            {77, "Tk_Repita"},
            {78, "Tk_Procedimento"},
            {79, "Tk_FimProcedimento"},
            {80, "Tk_Vazio"},
            {81, "Tk_Funcao"},
            {82, "Tk_FimFuncao"},
            {83, "Tk_Retorne"}
        };

        public static List<char> Letras = new List<char>()
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        public static List<char> Numeros = new List<char>()
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static List<char> Caracteres = new List<char>()
        {
            '_'
        };

        public static int Tipo(Char c)
        {
            if (LexMap.Numeros.Contains(c))
            {
                return LexMap.Consts["CONSTINTEIRO"];
            }
            if (LexMap.Letras.Contains(c))
            {
                return LexMap.Consts["ID"];
            }
            else if(c == ':')
            {
                return LexMap.Consts["DOISPONTOS"];
            }
            else if (c == '"')
            {
                return LexMap.Consts["STRING"];
            }
            else if (c == ',')
            {
                return LexMap.Consts["VIRGULA"];
            }
            else if (c == '<')
            {
                return LexMap.Consts["MENOR"];
            }
            else if (c == '>')
            {
                return LexMap.Consts["MAIOR"];
            }
            else if (c == '=')
            {
                return LexMap.Consts["IGUAL"];
            }
            else if (c == '+')
            {
                return LexMap.Consts["MAIS"];
            }
            else if (c == '-')
            {
                return LexMap.Consts["MENOS"];
            }
            else if (c == '*')
            {
                return LexMap.Consts["MULTIPLICACAO"];
            }
            else if (c == '/')
            {
                return LexMap.Consts["DIVISAO"];
            }
            else if (c == '(')
            {
                return LexMap.Consts["ABREPAR"];
            }
            else if (c == ')')
            {
                return LexMap.Consts["FECHAPAR"];
            }
            else if (c == '\\')
            {
                return LexMap.Consts["DIVC"];
            }
            else if (c == '%')
            {
                return LexMap.Consts["MODC"];
            }

            return 0;
        }

        public static string TokenGetNome(int _key)
        {
            String value = null;
            bool flag = LexMap.TokenNome.TryGetValue(_key, out value);

            if (!flag)
            {
                throw new Exception(String.Format("Token de chave {0} não encontrado", _key));    
            }

            return value;
        }
    }
}
