using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using static System.Console;
using System.Diagnostics;

namespace ModuleJogger
{
    class ModuleCppListener : CPP14BaseListener
    {

        private readonly MainWindow _window;
        private CPP14Parser _parser;
        private CommonTokenStream _tokens;
        private string _className = String.Empty;
        private string _nestedPath;
        private string _path;
        private string _fileName;
        private string _signature;
        private bool _hasBeenConsumed = false;

        public ModuleCppListener(MainWindow window, CPP14Parser parser, CommonTokenStream tokens, string nestedPath, string path, string fileName)
        {
            _window = window;
            _parser = parser;
            _tokens = tokens;
            _nestedPath = nestedPath;
            _path = path;
            _fileName = fileName;
        }

        // Override default listener behavior
        //void ExitKeyWord(CPP14Parser.KeywordContext context)
        //{
        //    Console.WriteLine("Oh, a key!");
        //}

        public override void EnterClassname([NotNull] CPP14Parser.ClassnameContext context)
        {
            base.EnterClassname(context);
            if (context.Identifier() != null)
            {
                string className = context.Identifier().ToString();
                if (MainWindow.IsDebugging)
                    WriteLine("EnterClassname: " + className);
                // Check if there is a waiting _signature to be consumed.
                if (!_hasBeenConsumed && _signature != null)
                {
                    if (!_signature.StartsWith(className))
                        _signature = className + _signature;
                    _window.AddModule(_nestedPath, _path, _fileName, _signature);
                    _hasBeenConsumed = true;
                    _signature = null;
                }
            }
        }

        public override void EnterClasshead([NotNull] CPP14Parser.ClassheadContext context)
        {
            base.EnterClasshead(context);
            if (MainWindow.IsDebugging)
            {
                //WriteLine("EnterClasshead: " + context.GetText());
                WriteLine("EnterClasshead: classheadname(): " + context.classheadname().GetText());
            }
            if (context != null && context.classheadname() != null && context.classheadname().GetText() != null)
            {
                _className = context.classheadname().GetText();
                _window.AddModule(_nestedPath, _path, _fileName, _className);
            }
        }

        /*
        public override void EnterMemberdeclarator([NotNull] CPP14Parser.MemberdeclaratorContext context)
        {
            base.EnterMemberdeclarator(context);
            //if (MainWindow.IsDebugging)
            //{
                if (context.Identifier() != null && context.Identifier().GetText().Length > 0)
                    WriteLine("EnterMemberdeclarator: " + context.Identifier().GetText());
                if (context.Identifier() != null && context.Identifier().ToString().Length > 0)
                    WriteLine("EnterMemberdeclarator: " + context.Identifier().ToString());
                if (context.attributespecifierseq() != null)
                    WriteLine("EnterMemberdeclarator attrib.:" + context.attributespecifierseq().GetText());
            //}
        }
        */

        /*
        public override void EnterMemberdeclaration([NotNull] CPP14Parser.MemberdeclarationContext context)
        {
            base.EnterMemberdeclaration(context);
            if (MainWindow.IsDebugging)
                WriteLine("EnterMemberdeclaration: " + context.GetText());
        }
        */

        /*
        public override void EnterFunctionspecifier([NotNull] CPP14Parser.FunctionspecifierContext context)
        {
            base.EnterFunctionspecifier(context);
            if (MainWindow.IsDebugging)
            {
                WriteLine("EnterFunctionspecifier: GetText(): " + context.GetText());
                WriteLine("EnterFunctionspecifier: ToString(): " + context.ToString());
            }
        }
        */

        public override void EnterFunctiondefinition([NotNull] CPP14Parser.FunctiondefinitionContext context)
        {
            base.EnterFunctiondefinition(context);
            if (MainWindow.IsDebugging)
            {
                //WriteLine("EnterFunctiondefinition: " + context.GetText());
                WriteLine("EnterFunctiondefinition declarator: " + context.declarator().GetText());
            }
            _signature = context.declarator().GetText();
            _hasBeenConsumed = false;
            if (MainWindow.IsDebugging)
            {
                Debug.WriteLine("EnterFunctiondefinition declarator: " + context.declarator().GetText());
                if (context.declarator().noptrdeclarator() != null)
                    Debug.WriteLine("EnterFunctiondefinition noptrdeclarator(): " + context.declarator().noptrdeclarator().GetText());
                if (context.declarator().ptrdeclarator() != null)
                    Debug.WriteLine("EnterFunctiondefinition ptrdeclarator(): " + context.declarator().ptrdeclarator().GetText());
                if (context.declarator().parametersandqualifiers() != null)
                    Debug.WriteLine("EnterFunctiondefinition parametersandqualifers(): " + context.declarator().parametersandqualifiers().GetText());
                if (context.declarator().trailingreturntype() != null)
                    Debug.WriteLine("EnterFunctiondefinition trailingreturntype(): " + context.declarator().trailingreturntype().GetText());
            }
        }

        /*
        public override void EnterDeclarator([NotNull] CPP14Parser.DeclaratorContext context)
        {
            base.EnterDeclarator(context);
        }

        public override void EnterDeclaratorid([NotNull] CPP14Parser.DeclaratoridContext context)
        {
            base.EnterDeclaratorid(context);
        }
        */

        /*
        public override void EnterParameterdeclaration([NotNull] CPP14Parser.ParameterdeclarationContext context)
        {
            base.EnterParameterdeclaration(context);
        }

        public override void EnterParameterdeclarationlist([NotNull] CPP14Parser.ParameterdeclarationlistContext context)
        {
            base.EnterParameterdeclarationlist(context);
        }

        public override void EnterFunctionbody([NotNull] CPP14Parser.FunctionbodyContext context)
        {
            base.EnterFunctionbody(context);
        }

        public override void EnterNoptrdeclarator([NotNull] CPP14Parser.NoptrdeclaratorContext context)
        {
            base.EnterNoptrdeclarator(context);
            if (MainWindow.IsDebugging)
            {
                if (context.declaratorid() != null)
                    WriteLine("EnterNoptrdeclarator: declaratorid().GetText(): " + context.declaratorid().GetText());
                WriteLine("EnterNoptrdeclarator: GetText(): " + context.GetText());
            }
        }

        public override void EnterPtrdeclarator([NotNull] CPP14Parser.PtrdeclaratorContext context)
        {
            base.EnterPtrdeclarator(context);
            if (MainWindow.IsDebugging)
            {
                if (context.ptrdeclarator() != null)
                    WriteLine("EnterPtrdeclarator: " + context.ptrdeclarator().GetText());
            }
        }
        */

        /*
        public override void EnterClass_definition([NotNull] CPP14Parser.Class_definitionContext context)
        {
            base.EnterClass_definition(context);
            //WriteLine("interface I" + context.identifier() + " {");
            //WriteLine("interface I" + context.ToString() + " {");
            //WriteLine("interface I" + context.identifier().ToString() + " {");
            //context.Start.StartIndex;

            WriteLine("EnterClass_definition: " + context.identifier().GetText());
        }

        public override void ExitClass_definition([NotNull] CSharp4Parser.Class_definitionContext context)
        {
            base.ExitClass_definition(context);
            //WriteLine("}");
        }
        */

    }
}
