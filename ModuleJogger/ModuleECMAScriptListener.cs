using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System.Diagnostics;
using System.Text;

namespace ModuleJogger
{
    class ModuleEcmaScriptListener : ECMAScriptBaseListener
    {
        private MainWindow _window;
        private ECMAScriptParser _parser;
        private CommonTokenStream _tokens;
        private string _className = String.Empty;
        private string _nestedPath;
        private string _path;
        private string _fileName;
        private string _signature;

        public ModuleEcmaScriptListener(MainWindow window, ECMAScriptParser parser, CommonTokenStream tokens, string nestedPath, string path, string fileName)
        {
            _window = window;
            _parser = parser;
            _tokens = tokens;
            _nestedPath = nestedPath;
            _path = path;
            _fileName = fileName;
        }

        public override void EnterFunctionDeclaration([NotNull] ECMAScriptParser.FunctionDeclarationContext context)
        {
            base.EnterFunctionDeclaration(context);
            if (MainWindow.IsDebugging)
            {
                Debug.WriteLine("EnterFunctionDeclaration: " + context.Identifier());
                Debug.WriteLine("EnterFunctionDeclaration Function(): " + context.Function().Parent.RuleContext.GetText());
                //Debug.WriteLine("EnterFunctionDeclaration: " + context.formalParameterList().Identifier().ToString());
                if (context.formalParameterList() != null && context.formalParameterList().Identifier() != null)
                    foreach (var node in context.formalParameterList().Identifier())
                    {
                        Debug.WriteLine("   node " + node.GetText());
                        //Debug.WriteLine("   nodeType " + node.GetType());
                    }

            }
            StringBuilder sig = new StringBuilder();
            sig.Append(context.Identifier());
            if (context.formalParameterList() != null && context.formalParameterList().Identifier() != null)
            {
                sig.Append("(");
                bool isAfterFirstArg = false;
                foreach (var node in context.formalParameterList().Identifier())
                {
                    if (isAfterFirstArg)
                        sig.Append(", ");
                    else
                        isAfterFirstArg = true;
                    sig.Append(node.GetText());
                }
                sig.Append(")");
            }
            else
                sig.Append("()");
            _signature = sig.ToString();
            _window.AddModule(_nestedPath, _path, _fileName, _signature);
        }

        public override void EnterFunctionExpression([NotNull] ECMAScriptParser.FunctionExpressionContext context)
        {
            // Identifier() will be null, since these functions are specified with the generic text "function"
            //if (MainWindow.IsDebugging)
            //    Debug.WriteLine("EnterFunctionExpression: " + context.Identifier());
            StringBuilder sig = new StringBuilder();
            // These functions are specified with the generic text "function"
            sig.Append("function");
            if (context.formalParameterList() != null && context.formalParameterList().Identifier() != null)
            {
                sig.Append("(");
                bool isAfterFirstArg = false;
                foreach (var node in context.formalParameterList().Identifier())
                {
                    if (isAfterFirstArg)
                        sig.Append(", ");
                    else
                        isAfterFirstArg = true;
                    sig.Append(node.GetText());
                }
                sig.Append(")");
            }
            else
                sig.Append("()");
            _signature = sig.ToString();
            _window.AddModule(_nestedPath, _path, _fileName, _signature);
        }

        public override void EnterFunctionBody([NotNull] ECMAScriptParser.FunctionBodyContext context)
        {
            base.EnterFunctionBody(context);
            //if (MainWindow.IsDebugging)
            //    Debug.WriteLine("EnterFunctionBody: " + context.GetText());
        }
    }
}
