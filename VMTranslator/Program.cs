using System;
using System.Collections.Generic;
using System.IO;

namespace VMTranslator
{
    internal class Program
    {
        public delegate void CommandDelegate(Parser parser, CodeWriter codeWriter);

        private static readonly Dictionary<CType, CommandDelegate> commands = new Dictionary<CType, CommandDelegate>(10)
        {
            {
                CType.CPush,
                (parser, codeWriter) => { codeWriter.WritePushPop(parser.CommandType(), parser.Arg1(), parser.Arg2()); }
            },
            {
                CType.CPop,
                (parser, codeWriter) => { codeWriter.WritePushPop(parser.CommandType(), parser.Arg1(), parser.Arg2()); }
            },
            {
                CType.CArithmetic,
                (parser, codeWriter) => { codeWriter.WriteArithmetic(parser.Arg1()); }
            },
            {
                CType.CLabel,
                (parser, codeWriter) => { codeWriter.WriteLabel(parser.Arg1()); }
            },
            {
                CType.CGoto,
                (parser, codeWriter) => { codeWriter.WriteGoto(parser.Arg1()); }
            },
            {
                CType.CIf, 
                (parser, codeWriter) => { codeWriter.WriteIf(parser.Arg1()); }
            },
            {
                CType.CFunction, 
                (parser, codeWriter) => { codeWriter.WriteFunction(parser.Arg1(), parser.Arg2()); }
            },
            {
                CType.CCall, 
                (parser, codeWriter) => { codeWriter.WriteCall(parser.Arg1(), parser.Arg2()); }
            },
            {
                CType.CReturn, 
                (parser, codeWriter) => { codeWriter.WriteReturn(); }
            },
        };


        static void ProcessFile(string path, string fileName, bool append)
        {
            var writerPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar +
                             fileName + ".asm";
            try
            {
                using (var sr = new StreamReader(path))
                {
                    using (var sw = new StreamWriter(writerPath, append))
                    {
                        var parser = new Parser(sr);
                        var codeWriter = new CodeWriter(sw, path);
                        if(!append) codeWriter.WriteInit();
                        while (parser.HasMoreCommands())
                        {
                            parser.Advance();
                            commands[parser.CommandType()].Invoke(parser, codeWriter);
                        }
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void ProcessDirectory(string path)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            string[] vmFiles = Directory.GetFiles(path, "*.vm");
            bool append = false;
            foreach (var item in vmFiles)
            {
                var _path = path + Path.DirectorySeparatorChar + Path.GetFileName(item);
                ProcessFile(_path, fileName, append);
                append = true;
                Console.WriteLine("{0} File Processed", _path);
            }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: VMTranslator <File>|<Directory>");
                return;
            }

            var path = args[0];

            if (File.Exists(path))
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                ProcessFile(path, fileName, false);
            }
            else if (Directory.Exists(path))
            {
                ProcessDirectory(path);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", path);
            }
        }
    }
}