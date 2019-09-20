using System;
using System.IO;

namespace VMTranslator
{
    public class AsmString
    {
        public static readonly string AddStr;
        public static readonly string SubStr;
        public static readonly string NegStr;
        public static readonly string EqStr;
        public static readonly string GtStr;
        public static readonly string LtStr;
        public static readonly string AndStr;
        public static readonly string OrStr;
        public static readonly string NotStr;
        public static readonly string PopStr;
        public static readonly string PushStr;
        public static readonly string PopTmpStr;
        public static readonly string PushTmpStr;
        public static readonly string PopPtrStr;
        public static readonly string PushPtrStr;
        public static readonly string PushStaticStr;
        public static readonly string PopStaticStr;
        public static readonly string GotoStr;
        public static readonly string IfStr;
        public static readonly string FuncStr;
        public static readonly string CallStr;
        public static readonly string ReturnStr;
        public static readonly string InitStr;

        static AsmString()
        {
            var path = @"C:\Users\Я\source\repos\VMTranslator\VMTranslator\ASMs\";
            var add = path + "Add.asm";
            var sub = path + "Sub.asm";
            var neg = path + "Neg.asm";
            var eq = path + "Eq.asm";
            var gt = path + "Gt.asm";
            var lt = path + "Lt.asm";
            var and = path + "And.asm";
            var or = path + "Or.asm";
            var not = path + "Not.asm";
            var pop = path + "Pop.asm";
            var push = path + "Push.asm";
            var popTmp = path + "PopTmp.asm";
            var pushTmp = path + "PushTmp.asm";
            var popPtr = path + "PopPtr.asm";
            var pushPtr = path + "PushPtr.asm";
            var pushStatic = path + "PushStatic.asm";
            var popStatic = path + "PopStatic.asm";
            var go = path + "Goto.asm";
            var _if = path + "If.asm";
            var func = path + "Function.asm";
            var call = path + "Call.asm";
            var ret = path + "Return.asm";
            var ini = path + "Init.asm";

            try
            {
                using (var reader = new StreamReader(add))
                {
                    AddStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(sub))
                {
                    SubStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(sub))
                {
                    SubStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(neg))
                {
                    NegStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(eq))
                {
                    EqStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(gt))
                {
                    GtStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(lt))
                {
                    LtStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(and))
                {
                    AndStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(or))
                {
                    OrStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(not))
                {
                    NotStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(pop))
                {
                    PopStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(push))
                {
                    PushStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(pushTmp))
                {
                    PushTmpStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(popTmp))
                {
                    PopTmpStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(pushPtr))
                {
                    PushPtrStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(popPtr))
                {
                    PopPtrStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(popStatic))
                {
                    PopStaticStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(pushStatic))
                {
                    PushStaticStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(go))
                {
                    GotoStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(_if))
                {
                    IfStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(func))
                {
                    FuncStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(call))
                {
                    CallStr = reader.ReadToEnd();
                }

                using (var reader = new StreamReader(ret))
                {
                    ReturnStr = reader.ReadToEnd();
                }
                
                using (var reader = new StreamReader(ini))
                {
                    InitStr = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
