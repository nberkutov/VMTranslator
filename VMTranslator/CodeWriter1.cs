using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VMTranslator
{
    class CodeWriter1
    {
        int eq = 0;
        int gt = 0;
        int lt = 0;
        //int addr = 0;
        int pop = 0;
        int push = 0;
        string fileName = String.Empty;

        Dictionary<string, string> segments;
        StreamWriter writer;
        AsmString asmString;

        public CodeWriter1(StreamWriter sw, string p)
        {
            writer = sw;
            segments = new Dictionary<string, string>(8);
            segments.Add("local", "LCL");
            segments.Add("argument", "ARG");
            segments.Add("this", "THIS");
            segments.Add("that", "THAT");
            new AsmString();
            fileName = Path.GetFileNameWithoutExtension(p);
            asmString = new AsmString();
        }

        public void WriteArithmetic(string command)
        {
            writer.WriteLine(@"//" + command);
            if (command == "add")
            {
                WriteLine(AsmString.addStr);
            }
            if (command == "sub")
            {
                writer.WriteLine(AsmString.subStr);
            }
            if (command == "neg")
            {
                WriteLine(AsmString.negStr);
            }
            if (command == "eq")
            {
                writer.WriteLine(AsmString.eqStr, eq);
                eq++;
            }
            if (command == "gt")
            {
                writer.WriteLine(AsmString.gtStr, gt);
                gt++;
            }
            if (command == "lt")
            {
                writer.WriteLine(AsmString.ltStr, lt);
                lt++;
            }
            if (command == "and")
            {
                WriteLine(AsmString.andStr);
            }
            if (command == "or")
            {
                WriteLine(AsmString.orStr);
            }
            if (command == "not")
            {
                WriteLine(AsmString.notStr);
            }
        }

        public void WritePushPop(C_TYPE command, string segment, int index)
        {
            if (command == C_TYPE.C_PUSH)
            {
                writer.WriteLine(@"//push {0} {1}", segment, index);
                if (segment == "constant")
                {
                    WriteLine("@" + index);
                    WriteLine("D=A");
                    WriteLine("@SP");
                    WriteLine("A=M");
                    WriteLine("M=D");
                    WriteLine("@SP");
                    WriteLine("M=M+1");
                }
                if (segment == "local" || segment == "argument" || segment == "this" || segment == "that")
                {
                    writer.WriteLine(AsmString.pushStr, segments[segment], index);
                }
                if (segment == "temp")
                {
                    writer.WriteLine(AsmString.pushTmpStr, index);
                }
                if (segment == "pointer")
                {
                    writer.WriteLine(AsmString.pushPtrStr, index, push);
                    push++;
                }
                if (segment == "static")
                {
                    writer.WriteLine(AsmString.pushStaticStr, fileName, index);
                }
            }
            if (command == C_TYPE.C_POP)
            {
                writer.WriteLine(@"//pop {0} {1}", segment, index);
                if (segment == "local" || segment == "argument" || segment == "this" || segment == "that")
                {
                    writer.WriteLine(AsmString.popStr, segments[segment], index);
                }
                if (segment == "temp")
                {
                    writer.WriteLine(AsmString.popTmpStr, index);
                }
                if (segment == "pointer")
                {
                    writer.WriteLine(AsmString.popPtrStr, index, pop, pop);
                    pop++;
                }
                if (segment == "static")
                {
                    writer.WriteLine(AsmString.popStaticStr, fileName, index);
                }
            }
        }

        private void WriteLine(string arg)
        {
            writer.WriteLine(arg);
        }
    }
}
