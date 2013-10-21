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
            if (LineManager.Instance.ReadLine())
            {
                AnalisadorLexico.Analisar();
            }

            return "";
        }
    }
}