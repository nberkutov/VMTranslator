using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VMTranslator {
    
    /// <summary>
    /// Производит чтение команд виртуальной машины из файла с расширением .vm,
    /// разбирает их на лексические компоненты и предоставляет к ним доступ. 
    /// </summary>
    class Parser {
        public string[] Current => _current;
        private string[] _current;
        
        private readonly StreamReader _reader;
        private readonly Dictionary<string, TypeDelegate> _commandTypes;
        private delegate CType TypeDelegate();
        
        public Parser(StreamReader sr)
        {
            _reader = sr;
            _current = null;
            _commandTypes = new Dictionary<string, TypeDelegate>(20)
            {
                {"add", () => CType.CArithmetic},
                {"sub", () => CType.CArithmetic},
                {"neg", () => CType.CArithmetic},
                {"eq", () => CType.CArithmetic},
                {"gt", () => CType.CArithmetic},
                {"lt", () => CType.CArithmetic},
                {"or", () => CType.CArithmetic},
                {"not", () => CType.CArithmetic},
                {"and", () => CType.CArithmetic},
                
                {"push", () => CType.CPush},
                {"pop", () => CType.CPop},
                {"label", () => CType.CLabel},
                {"goto", () => CType.CGoto},
                {"if-goto", () => CType.CIf},
                {"function", () => CType.CFunction},
                {"call", () => CType.CCall},
                {"return", () => CType.CReturn}
            };
        }

        /// <summary>
        /// Определяет, содержит ли файл еще команды, или достигнут его конец 
        /// </summary>
        /// <returns>bool</returns>
        public bool HasMoreCommands() 
        {
            return !_reader.EndOfStream;
        }

        /// <summary>
        /// Переходит к следующей команде
        /// </summary>
        public void Advance() 
        {
            StringBuilder sb = new StringBuilder(String.Empty);
            char c = ' ';
            while (c != '\n') 
            {
                c = (char)_reader.Read();
                if (c == '/') 
                {
                    while (c != '\n') 
                    {
                        c = (char)_reader.Read();
                    }
                    break;
                }
                if (c != '\n' && c != '\r') 
                {
                    sb.Append(c);
                }

            }
            if (sb.Length > 0)
                _current = sb.ToString().Split(' ');
            else
                Advance();
        }
        
        /// <summary>
        /// Возвращает тип комманды
        /// </summary>
        /// <returns></returns>
        public CType CommandType() 
        {
            return _commandTypes[_current[0]].Invoke();
        }

        
        /// <summary>
        /// Возвращает первый аргумент команды.
        /// Если команда является арифметической, то само название команды
        /// </summary>
        /// <returns></returns>
        public string Arg1()
        {
            return CommandType() == CType.CArithmetic ? _current[0] : _current[1];
        }

        /// <summary>
        /// Возвращает второй аргумент команды
        /// </summary>
        /// <returns></returns>
        public int Arg2()
        {
            if (CommandType() == CType.CPop || CommandType() == CType.CPush || CommandType() == CType.CFunction || CommandType() == CType.CCall) 
            {
                return int.Parse(_current[2]);
            }
            return -1;
        }
    }
}
