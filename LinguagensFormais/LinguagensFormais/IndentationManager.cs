using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompiladoresTrabalho
{
    public class IndentationManager
    {
        public Int32 IndenterCount { get; set; }
        public String IndenterCharacter { get; set; }

        private static IndentationManager instance { get; set; }

        public static IndentationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IndentationManager();
                }

                return instance;
            }
        }

        public IndentationManager()
        {
            this.IndenterCount = 0;
            this.IndenterCharacter = "\t";
        }

        public void Increase()
        {
            this.IndenterCount++;
        }

        public void Decrement()
        {
            this.IndenterCount--;

            if (this.IndenterCount < 0)
                this.IndenterCount = 0;
        }

        public string GetIndentation()
        {
            string _return = String.Empty;

            for (int i = 0; i < this.IndenterCount; i++)
            {
                _return += this.IndenterCharacter;
            }

            return _return;
        }
    }
}
