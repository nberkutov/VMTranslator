using System.Collections.Generic;
using System.IO;

namespace VMTranslator
{
    public class CodeWriter3
    {
        
        public static int eqCounter;
        public static int gtCounter;
        public static int ltCounter;
        public static int popCounter;
        public static int pushCounter;
        public static int retAddrCount;

        public static string FileName => _fileName;

        private static string _fileName = string.Empty;
        private string _curFunc = string.Empty;

        public static readonly Dictionary<string, string> MemorySegmentPointers;

        private readonly StreamWriter _writer;
        private delegate void CommandDelegate(StreamWriter writer);
        private delegate void ThreeArgCommandDelegate(StreamWriter writer, string segment, int index);
        
        private readonly Dictionary<string, CommandDelegate> _commandsToWriteArithmetic;
        private readonly Dictionary<string, ThreeArgCommandDelegate> _commandsToWritePush;
        private readonly Dictionary<string, ThreeArgCommandDelegate> _commandsToWritePop;

        #region Delegates
        
        private readonly CommandDelegate _addCommand = writer => writer.WriteLine(AsmString.AddStr);
        private readonly CommandDelegate _subCommand = writer => writer.WriteLine(AsmString.SubStr);
        private readonly CommandDelegate _negCommand = writer => writer.WriteLine(AsmString.AddStr);
        private readonly CommandDelegate _andCommand = writer => writer.WriteLine(AsmString.AndStr);
        private readonly CommandDelegate _orCommand = writer => writer.WriteLine(AsmString.OrStr);
        private readonly CommandDelegate _notCommand = writer => writer.WriteLine(AsmString.NotStr);
        
        private readonly CommandDelegate _eqCommand = writer =>
        {
            writer.WriteLine(AsmString.EqStr, eqCounter);
            eqCounter++;
        };
        
        private readonly CommandDelegate _gtCommand = writer =>
        {
            writer.WriteLine(AsmString.GtStr, gtCounter);
            gtCounter++;
        };
        
        private readonly CommandDelegate _ltCommand = writer =>
        {
            writer.WriteLine(AsmString.LtStr, ltCounter);
            ltCounter++;
        };
        
        private readonly ThreeArgCommandDelegate _pushConstant = (writer, segment, index) =>
        {
            writer.WriteLine("@" + index);
            writer.WriteLine("D=A");
            writer.WriteLine("@SP");
            writer.WriteLine("A=M");
            writer.WriteLine("M=D");
            writer.WriteLine("@SP");
            writer.WriteLine("M=M+1");
        };

        private readonly ThreeArgCommandDelegate _pushDefaultCommand = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PushStr, MemorySegmentPointers[segment], index);
        };

        private readonly ThreeArgCommandDelegate _pushTemp = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PushTmpStr, index);
        };

        private readonly ThreeArgCommandDelegate _pushPointer = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PushPtrStr, index, pushCounter, pushCounter);
            pushCounter++;
        };
        
        private readonly ThreeArgCommandDelegate _pushStatic = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PushStaticStr, _fileName, index);
        };
        
        private readonly ThreeArgCommandDelegate _popDefault = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PopStr, MemorySegmentPointers[segment], index);
        };
        
        private readonly ThreeArgCommandDelegate _popTemp = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PopTmpStr, index);
        };
        
        private readonly ThreeArgCommandDelegate _popPointer = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PopPtrStr, index, popCounter, popCounter);
            popCounter++;
        };
        
        private readonly ThreeArgCommandDelegate _popStatic = (writer, segment, index) =>
        {
            writer.WriteLine(AsmString.PopStaticStr, _fileName, index);
        };
        #endregion

        static CodeWriter3()
        {
            MemorySegmentPointers = new Dictionary<string, string>(8)
            {
                {"local", "LCL"}, {"argument", "ARG"}, {"this", "THIS"}, {"that", "THAT"}
            };
        }
        
        public CodeWriter3(StreamWriter sw, string path)
        {
            _writer = sw;
            _fileName = Path.GetFileNameWithoutExtension(path);
            
            _commandsToWriteArithmetic = new Dictionary<string, CommandDelegate>(9)
            {
                {"add", _addCommand},
                {"sub", _subCommand},
                {"neg", _negCommand},
                {"eq", _eqCommand},
                {"gt", _gtCommand},
                {"lt", _ltCommand},
                {"and", _andCommand},
                {"or", _orCommand},
                {"not", _notCommand}
            };
            
            _commandsToWritePush = new Dictionary<string, ThreeArgCommandDelegate>(8)
            {
                {"constant", _pushConstant},
                {"local", _pushDefaultCommand},
                {"argument", _pushDefaultCommand},
                {"this", _pushDefaultCommand},
                {"that", _pushDefaultCommand},
                {"temp", _pushTemp},
                {"pointer", _pushPointer},
                {"static", _pushStatic}
            };
            
            _commandsToWritePop = new Dictionary<string, ThreeArgCommandDelegate>(7)
            {
                {"local", _popDefault},
                {"argument", _popDefault},
                {"this", _popDefault},
                {"that", _popDefault},
                {"temp", _popTemp},
                {"pointer", _popPointer},
                {"static", _popStatic}
            };


        }

        public void WriteArithmetic(string command)
        {
            _writer.WriteLine(@"//" + command); //Debug output
            
            _commandsToWriteArithmetic[command] (_writer);
        }

        public void WritePushPop(CType command, string segment, int index)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (command)
            {
                case CType.CPush:
                    _writer.WriteLine(@"//push {0} {1}", segment, index);
                    _commandsToWritePush[segment] (_writer, segment, index);
                    break;
                case CType.CPop:
                    _writer.WriteLine(@"//pop {0} {1}", segment, index);
                    _commandsToWritePop[segment] (_writer, segment, index);
                    break;
            }
        }

        public void WriteInit()
        {

        }

        public void WriteLabel(string label)
        {
           // _writer.WriteLine("//label {0}", label);
            _writer.WriteLine("({0})", label);
        }

        public void WriteGoto(string label)
        {
            //_writer.WriteLine("//GOTO {0}", label);
            _writer.WriteLine(AsmString.GotoStr, label);
        }

        public void WriteIf(string label)
        {
            _writer.WriteLine("//If {0}", label);
            _writer.WriteLine(AsmString.IfStr, label);
        }

        public void WriteFunction(string functionName, int numVars)
        {
            _writer.WriteLine("//Function {0} {1}", functionName, numVars);
            _writer.WriteLine(AsmString.FuncStr, functionName, numVars);
        }

        public void WriteCall(string functionName, int numVars)
        {
            _writer.WriteLine("//Call {0} {1}", functionName, numVars);
            _writer.WriteLine(AsmString.CallStr, functionName, retAddrCount, numVars);
            retAddrCount++;
        }

        public void WriteReturn()
        {
            _writer.WriteLine("//Return {0}", retAddrCount);
            _writer.WriteLine(AsmString.ReturnStr, retAddrCount);
            
        }
    }
}
