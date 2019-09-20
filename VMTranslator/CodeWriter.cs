using System;
using System.Collections.Generic;
using System.IO;

namespace VMTranslator
{
    /// <summary>
    /// Предоставляет набор методов для записи команд виртуальной машины,
    /// транслированных в ассемблерный код
    /// </summary>
    public class CodeWriter
    {
        //Щетчики для создания уникальных индетификаторов вызовов инструкций
        private int eqCounter;
        private int gtCounter;
        private int ltCounter;
        private int popCounter;
        private int pushCounter;
        private int retAddrCount;

        private string fileName;

        //Имя обрабатываемой в данный момент функции для создания правильных имен меток и переменных
        private string currentFunction;

        private readonly StreamWriter writer;

        private delegate void CommandDelegate(StreamWriter writer);

        private delegate void ThreeArgCommandDelegate(StreamWriter writer, string segment, int index);

        private readonly Dictionary<string, CommandDelegate> arithmeticCommands;
        private readonly Dictionary<string, ThreeArgCommandDelegate> pushCommands;
        private readonly Dictionary<string, ThreeArgCommandDelegate> popCommands;

        public static readonly Dictionary<string, string>
            memorySegmentPointers; //Имена сегментов памяти <в .vm-файле, в ASM-коде>

        static CodeWriter()
        {
            memorySegmentPointers = new Dictionary<string, string>(8)
            {
                {"local", "LCL"}, {"argument", "ARG"}, {"this", "THIS"}, {"that", "THAT"}
            };
        }


        /// <param name="sw"></param>
        /// <param name="path"></param>
        public CodeWriter(StreamWriter sw, string path)
        {
            currentFunction = string.Empty;
            writer = sw;
            fileName = Path.GetFileNameWithoutExtension(path);

            ThreeArgCommandDelegate pushDefault = (writer, segment, index) =>
            {
                writer.WriteLine("@{0}", memorySegmentPointers[segment]);
                writer.WriteLine("D=M");
                writer.WriteLine("@{0}", index);
                writer.WriteLine("D=D+A");
                writer.WriteLine("@addr");
                writer.WriteLine("M=D");
                writer.WriteLine("A=M");
                writer.WriteLine("D=M");
                writer.WriteLine("@SP");
                writer.WriteLine("A=M");
                writer.WriteLine("M=D");
                writer.WriteLine("@SP");
                writer.WriteLine("M=M+1");
            };
            ThreeArgCommandDelegate popDefault = (writer, segment, index) =>
            {
                writer.WriteLine("@{0}", memorySegmentPointers[segment]);
                writer.WriteLine("D=M");
                writer.WriteLine("@{0}", index);
                writer.WriteLine("D=D+A");
                writer.WriteLine("@addr");
                writer.WriteLine("M=D");
                writer.WriteLine("@SP");
                writer.WriteLine("M=M-1");
                writer.WriteLine("A=M");
                writer.WriteLine("D=M");
                writer.WriteLine("@addr");
                writer.WriteLine("A=M");
                writer.WriteLine("M=D");
            };
            
            arithmeticCommands = new Dictionary<string, CommandDelegate>(9)
            {
                {"add", writer => 
                {
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("D=M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("M=M+D");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M+1");
                }},
                {"sub", writer =>
                {
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("D=M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("M=M-D");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M+1");
                }},
                {"neg", writer =>
                {
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("M=-M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M+1");

                }},
                {
                    "eq", writer =>
                    {
                        #region Writing Long Asm

                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M-D;");
                        writer.WriteLine("@TRUE_EQ{0}", eqCounter);
                        writer.WriteLine("D;JEQ");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=0");
                        writer.WriteLine("@CONTINUE_EQ{0}", eqCounter);
                        writer.WriteLine("0;JMP");
                        writer.WriteLine("(TRUE_EQ{0})", eqCounter);
                        writer.WriteLine("@1");
                        writer.WriteLine("D=-A");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("(CONTINUE_EQ{0})", eqCounter);
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M+1");

                        #endregion
                        
                        eqCounter++;
                    }
                },
                {
                    "gt", writer =>
                    {
                        #region Wrting Long Asm

                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M-D");
                        writer.WriteLine("@TRUE_GT{0}", gtCounter);
                        writer.WriteLine("D;JGT");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=0");
                        writer.WriteLine("@CONTINUE_GT{0}", gtCounter);
                        writer.WriteLine("0;JMP");
                        writer.WriteLine("(TRUE_GT{0})", gtCounter);
                        writer.WriteLine("@1");
                        writer.WriteLine("D=-A");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("(CONTINUE_GT{0})", gtCounter);
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M+1");

                        #endregion
                        gtCounter++;
                    }
                },
                {
                    "lt", writer =>
                    {
                        #region Writing Long Asm

                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M-D");
                        writer.WriteLine("@TRUE_LT{0}", ltCounter);
                        writer.WriteLine("D;JLT");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=0");
                        writer.WriteLine("@CONTINUE_LT{0}", ltCounter);
                        writer.WriteLine("0;JMP");
                        writer.WriteLine("(TRUE_LT{0})", ltCounter);
                        writer.WriteLine("@1");
                        writer.WriteLine("D=-A");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("(CONTINUE_LT{0})", ltCounter);
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M+1");

                        #endregion
                        ltCounter++;
                    }
                },
                {"and", writer =>
                {
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("D=M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("M=D&M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M+1");
                }},
                {"or", writer =>
                {
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("D=M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("M=D|M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M+1");

                }},
                {"not", writer =>
                {
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M-1");
                    writer.WriteLine("A=M");
                    writer.WriteLine("M=!M");
                    writer.WriteLine("@SP");
                    writer.WriteLine("M=M+1");

                }}
            };

            pushCommands = new Dictionary<string, ThreeArgCommandDelegate>(8)
            {
                {
                    "constant", (writer, segment, index) =>
                    {
                        writer.WriteLine("@" + index);
                        writer.WriteLine("D=A");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M+1");
                    }
                },
                {
                    "local",
                    (writer, segment, index) => { pushDefault(writer, segment, index); }
                },
                {
                    "argument",
                    (writer, segment, index) => { pushDefault(writer, segment, index); }
                },
                {
                    "this",
                    (writer, segment, index) => { pushDefault(writer, segment, index); }
                },
                {
                    "that",
                    (writer, segment, index) => { pushDefault(writer, segment, index); }
                },
                {
                    "temp", (writer, segment, index) =>
                    {
                        writer.WriteLine("@5");
                        writer.WriteLine("D=A");
                        writer.WriteLine("@{0}", index);
                        writer.WriteLine("D=D+A");
                        writer.WriteLine("@addr");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@addr");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                    }
                },
                {
                    "pointer", (writer, segment, index) =>
                    {
                        #region Writing Quite Long Asm

                        writer.WriteLine("@{0}", index);
                        writer.WriteLine("D=A");
                        writer.WriteLine("@this_push{0}", pushCounter);
                        writer.WriteLine("D;JEQ");
                        writer.WriteLine("@THAT");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@CONTINUE_push{0}", pushCounter);
                        writer.WriteLine("0;JMP");
                        writer.WriteLine("(this_push{0})", pushCounter);
                        writer.WriteLine("@THIS");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("(CONTINUE_push{0})", pushCounter);
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M+1");

                        #endregion
                        pushCounter++;
                    }
                },
                {
                    "static", (writer, segment, index) =>
                    {
                        writer.WriteLine("@{0}.{1}", fileName, index);
                        writer.WriteLine("D=M");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M+1");
                    }
                }
            };

            popCommands = new Dictionary<string, ThreeArgCommandDelegate>(7)
            {
                {
                    "local",
                    (writer, segment, index) => { popDefault(writer, segment, index); }
                },
                {
                    "argument",
                    (writer, segment, index) => { popDefault(writer, segment, index); }
                },
                {
                    "this",
                    (writer, segment, index) => { popDefault(writer, segment, index); }
                },
                {
                    "that",
                    (writer, segment, index) => { popDefault(writer, segment, index); }
                },
                {
                    "temp", (writer, segment, index) =>
                    {
                        writer.WriteLine("@5");
                        writer.WriteLine("D=A");
                        writer.WriteLine("@{0}", index);
                        writer.WriteLine("D=D+A");
                        writer.WriteLine("@addr");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@addr");
                        writer.WriteLine("A=M");
                        writer.WriteLine("M=D");
                    }
                },
                {
                    "pointer", (writer, segment, index) =>
                    {
                        writer.WriteLine("@SP");
                        writer.WriteLine("M=M-1");
                        writer.WriteLine("@{0}", index);
                        writer.WriteLine("D=A");
                        writer.WriteLine("@this_pop{0}", popCounter);
                        writer.WriteLine("D;JEQ");
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@THAT");
                        writer.WriteLine("M=D");
                        writer.WriteLine("@CONTINUE_pop{0}", popCounter);
                        writer.WriteLine("0;JMP");
                        writer.WriteLine("(this_pop{0})", popCounter);
                        writer.WriteLine("@SP");
                        writer.WriteLine("A=M");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@THIS");
                        writer.WriteLine("M=D");
                        writer.WriteLine("(CONTINUE_pop{0})", popCounter);

                        popCounter++;
                    }
                },
                {
                    "static", (writer, segment, index) =>
                    {
                        writer.WriteLine("@SP");
                        writer.WriteLine("AM=M-1");
                        writer.WriteLine("D=M");
                        writer.WriteLine("@{0}.{1}", fileName, index);
                        writer.WriteLine("M=D");
                    }
                }
            };
        }

        /// <summary>
        /// Запись арифметической команды
        /// </summary>
        /// <param name="command">Имя команды</param>
        public void WriteArithmetic(string command)
        {
            writer.WriteLine(@"//" + command); //Debug output
            arithmeticCommands[command].Invoke(writer);
        }


        /// <summary>
        /// Запись команд добавления на стек и удаления из стека
        /// </summary>
        /// <param name="command">Тип команды</param>
        /// <param name="segment">Сегмент памяти</param>
        /// <param name="index">Индекс</param>
        public void WritePushPop(CType command, string segment, int index)
        {
            switch (command)
            {
                case CType.CPush:
                    writer.WriteLine(@"//push {0} {1}", segment, index);
                    pushCommands[segment].Invoke(writer, segment, index);
                    break;
                case CType.CPop:
                    writer.WriteLine(@"//pop {0} {1}", segment, index);
                    popCommands[segment].Invoke(writer, segment, index);
                    break;
            }
        }

        /// <summary>
        /// Устанавливает значение указателя стека равным 256
        /// Вызывает системную функцию Sys.init
        /// </summary>
        public void WriteInit()
        {
            writer.WriteLine("@256");
            writer.WriteLine("D=A");
            writer.WriteLine("@SP");
            writer.WriteLine("M=D");
            WriteCall("Sys.init", 0);
        }

        /// <summary>
        /// Записывает метку
        /// </summary>
        /// <param name="label"></param>
        public void WriteLabel(string label)
        {
            writer.WriteLine("//label {0}", currentFunction + "$" + label);
            writer.WriteLine("({0})", currentFunction + "$" + label);
        }

        /// <summary>
        /// Записывает инструкцию goto
        /// </summary>
        /// <param name="label"></param>
        public void WriteGoto(string label)
        {
            writer.WriteLine("//GOTO {0}", label);

            writer.WriteLine("@{0}", currentFunction + "$" + label);
            writer.WriteLine("0;JMP");
        }

        /// <summary>
        /// Записывает инструкцию ветвления
        /// </summary>
        /// <param name="label"></param>
        public void WriteIf(string label)
        {
            writer.WriteLine("//If {0}", currentFunction + "$" + label);
            writer.WriteLine("@SP");
            writer.WriteLine("M=M-1");
            writer.WriteLine("A=M");
            writer.WriteLine("D=M");
            writer.WriteLine("@{0}", currentFunction + "$" + label);
            writer.WriteLine("D;JNE");
        }

        /// <summary>
        /// Запись объявления функции
        /// </summary>
        /// <param name="functionName">Имя функции</param>
        /// <param name="numVars">Количество аргументов функции на стеке</param>
        public void WriteFunction(string functionName, int numVars)
        {
            currentFunction = functionName;
            writer.WriteLine("//Function {0} {1}", functionName, numVars);
            writer.WriteLine("({0})", functionName);
            for (int i = 0; i < numVars; i++)
            {
                writer.WriteLine("@SP");
                writer.WriteLine("A=M");
                writer.WriteLine("M=0");
                writer.WriteLine("@SP");
                writer.WriteLine("M=M+1");
            }
        }

        /// <summary>
        /// Записывает вызов функции
        /// </summary>
        /// <param name="functionName">Имя функции</param>
        /// <param name="numVars">Количество аргументов функции на стеке</param>
        public void WriteCall(string functionName, int numVars)
        {
            writer.WriteLine("//Call {0} {1}", functionName, numVars);

            #region Writing Long Asm
            
            writer.WriteLine("@{0}$RET${1}", functionName, retAddrCount);
            writer.WriteLine("D=A");
            writer.WriteLine("@SP");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("M=M+1");
            writer.WriteLine("@LCL");
            writer.WriteLine("D=M");
            writer.WriteLine("@SP");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("M=M+1");
            writer.WriteLine("@ARG");
            writer.WriteLine("D=M");
            writer.WriteLine("@SP");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("M=M+1");
            writer.WriteLine("@THIS");
            writer.WriteLine("D=M");
            writer.WriteLine("@SP");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("M=M+1");
            writer.WriteLine("@THAT");
            writer.WriteLine("D=M");
            writer.WriteLine("@SP");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("M=M+1");
            writer.WriteLine("@SP");
            writer.WriteLine("D=M");
            writer.WriteLine("@5");
            writer.WriteLine("D=D-A");
            writer.WriteLine("@{0}", numVars);
            writer.WriteLine("D=D-A");
            writer.WriteLine("@ARG");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("D=M");
            writer.WriteLine("@LCL");
            writer.WriteLine("M=D");
            writer.WriteLine("@{0}", functionName);
            writer.WriteLine("0;JMP");
            writer.WriteLine("({0}$RET${1})", functionName, retAddrCount);
            
            #endregion
            
            retAddrCount++;
        }

        /// <summary>
        /// Return
        /// Записывает инструкцию возврата из функции
        /// </summary>
        public void WriteReturn()
        {
            writer.WriteLine("//Return");

            #region Writing Long Asm

            writer.WriteLine("@LCL");
            writer.WriteLine("D=M");
            writer.WriteLine("@R13");
            writer.WriteLine("M=D");
            writer.WriteLine("@5");
            writer.WriteLine("A=D-A");
            writer.WriteLine("D=M");
            writer.WriteLine("@R14");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("AM=M-1");
            writer.WriteLine("D=M");
            writer.WriteLine("@ARG");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@ARG");
            writer.WriteLine("D=M+1");
            writer.WriteLine("@SP");
            writer.WriteLine("M=D");
            writer.WriteLine("@R13");
            writer.WriteLine("AM=M-1");
            writer.WriteLine("D=M");
            writer.WriteLine("@THAT");
            writer.WriteLine("M=D");
            writer.WriteLine("@R13");
            writer.WriteLine("AM=M-1");
            writer.WriteLine("D=M");
            writer.WriteLine("@THIS");
            writer.WriteLine("M=D");
            writer.WriteLine("@R13");
            writer.WriteLine("AM=M-1");
            writer.WriteLine("D=M");
            writer.WriteLine("@ARG");
            writer.WriteLine("M=D");
            writer.WriteLine("@R13");
            writer.WriteLine("AM=M-1");
            writer.WriteLine("D=M");
            writer.WriteLine("@LCL");
            writer.WriteLine("M=D");
            writer.WriteLine("@R14");
            writer.WriteLine("A=M");
            writer.WriteLine("0;JMP");


            #endregion
        }
    }
}