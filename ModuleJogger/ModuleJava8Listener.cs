using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

using static System.Console;
using System.Linq;

namespace ModuleJogger
{
    /// <summary>
    /// Figuring  out which methods to subclass and listen to what largely a process of
    /// trial and error, which the trails can be seen in commented out code below.
    /// </summary>
    class ModuleJava8Listener : Java8BaseListener
    {

        private MainWindow _window;
        private Java8Parser _parser;
        private CommonTokenStream _tokens;
        private string _className = String.Empty;
        // This stack is used to hold the containing class names (or $ANON for anonymous classes)
        // for anonymous and inner classes.
        private Stack<string> _classHierarchyStack = new Stack<string>(20);
        private string _nestedPath;
        private string _path;
        private string _fileName;
        private string _signature;

        public ModuleJava8Listener(MainWindow window, Java8Parser parser, CommonTokenStream tokens, string nestedPath, string path, string fileName)
        {
            _window = window;
            _parser = parser;
            _tokens = tokens;
            _nestedPath = nestedPath;
            _path = path;
            _fileName = fileName;
        }

        /*
        public override void EnterClassDeclaration([NotNull] Java8Parser.ClassDeclarationContext context)
        {
            ParserRuleContext cdecl, parent;
            CommonToken start, stop;
            cdecl = parent = (Antlr4.Runtime.ParserRuleContext)context.Parent;  //classDeclaration
            WriteLine("name = " + Java8Parser.ruleNames[parent.RuleIndex]);
            parent = (ParserRuleContext)parent.Parent;   //typeDeclaration or member
            WriteLine("name = " + Java8Parser.ruleNames[parent.RuleIndex]);

            if (Java8Parser.ruleNames[parent.RuleIndex] == "typeDeclaration")
            {
                //List<Java8Parser.NormalClassDeclarationContext> mods =
                //    ((Java8Parser.TypeDeclarationContext) parent).classDeclaration();
                //if (((Java8Parser.TypeDeclarationContext)parent)
                parent = (ParserRuleContext) parent.Parent;
                start = (CommonToken) parent.Start;
            }
            else if (Java8Parser.ruleNames[parent.RuleIndex] == "member")
            {
                parent = (ParserRuleContext) parent.Parent;  //classBodyDeclaration
                start = (CommonToken)parent.Start;
            }
            else
            {
                start = (CommonToken)context.Start;
            }

            stop = (CommonToken)context.Stop;
            WriteLine("   *> " + _tokens.GetText(start, stop));

            //List<TerminalNode> terminalNodes = new ArrayList<TerminalNode>();
            //for (int i = 0; i < context.getChildCount(); i++)
            //{
            //    if (context.getChild(i) instanceof TerminalNode) {
            //    terminalNodes.add((TerminalNode)context.getChild(i));
            //}
        }
        */

        /*
        public override void EnterClassBodyDeclaration([NotNull] Java8Parser.ClassBodyDeclarationContext context)
        {
            
            //also .constructorMemberDeclaration
            if (!(context.classMemberDeclaration() != null && context.classMemberDeclaration().methodDeclaration() != null))
            {
                // No method declaration.
                return;
            }
            //String methodName = context.classMemberDeclaration().methodDeclaration().Identifier().getText();
            String methodName = context.classMemberDeclaration().methodDeclaration().GetText();
            //foreach (Java8Parser.ArgumentListContext arg in context.classMemberDeclaration().methodDeclaration().methodModifier())
            foreach (
                Java8Parser.MethodModifierContext arg in
                    context.classMemberDeclaration().methodDeclaration().methodModifier())
            {
                WriteLine("EnterClassBodyDeclaration: " + arg.GetText());
            }

            //Java8Parser.TypeParameterListContext ctx =
            //    context.classMemberDeclaration().methodDeclaration().methodHeader().typeParameters().typeParameterList();
            //WriteLine("EnterClassBodyDeclaration: " + ctx.ToString());

            //foreach (
            //Java8Parser.MethodHeaderContext arg in context.classMemberDeclaration().methodDeclaration().methodHeader().typeParameters())
            //Java8Parser.TypeParameterListContext arg in context.classMemberDeclaration().methodDeclaration().methodHeader().typeParameters().typeParameterList())


            //foreach (Java8Parser. mctx : context.)
            //{
            //    if (mctx.classOrInterfaceModifier() != null && mctx.classOrInterfaceModifier().annotation() != null)
            //   {
            //        System.out.println(methodName + " -> " + mctx.classOrInterfaceModifier().annotation().getText());
            //    }
            //} 
        }
        */

        /*
        public override void EnterClassDeclaration([NotNull] Java8Parser.ClassDeclarationContext context)
        {
            base.EnterClassDeclaration(context);
            if (MainWindow.IsDebugging)
                WriteLine("EnterClassDeclaration: " + context);
        }
        */

        /*
        public override void EnterClassType([NotNull] Java8Parser.ClassTypeContext context)
        {
            base.EnterClassType(context);
            if (MainWindow.IsDebugging)
                WriteLine("EnterClassType: " + context.GetText());
        }

        public override void EnterUnannClassType([NotNull] Java8Parser.UnannClassTypeContext context)
        {
            base.EnterUnannClassType(context);
            if (MainWindow.IsDebugging)
                WriteLine("EnterUnannClassType: " + context.GetText());
        }
        */

        public override void EnterClassInstanceCreationExpression([NotNull] Java8Parser.ClassInstanceCreationExpressionContext context)
        {
            base.EnterClassInstanceCreationExpression(context);
            if (MainWindow.IsDebugging)
            {
                WriteLine("EnterClassInstanceCreationExpression: " + context.GetText());
                if (context.classBody() != null)
                    WriteLine("EnterClassInstanceCreationExpression body: " + context.classBody().GetText());
            }
            if (context.classBody() != null && context.classBody().GetText().Length > 0)
            {
                //_classHierarchyStack.Push(_className);
                _classHierarchyStack.Push(_signature);
                _className = "$ANON";
                _classHierarchyStack.Push("$ANON");
            }
        }

        public override void ExitClassInstanceCreationExpression([NotNull] Java8Parser.ClassInstanceCreationExpressionContext context)
        {
            base.ExitClassInstanceCreationExpression(context);
            if (_classHierarchyStack.Count > 0)
                _className = _classHierarchyStack.Pop();
        }

        public override void EnterClassInstanceCreationExpression_lf_primary([NotNull] Java8Parser.ClassInstanceCreationExpression_lf_primaryContext context)
        {
            base.EnterClassInstanceCreationExpression_lf_primary(context);
            if (MainWindow.IsDebugging)
            {
                WriteLine("EnterClassInstanceCreationExpression_lf_primary: " + context.GetText());
                if (context.classBody() != null)
                    WriteLine("EnterClassInstanceCreationExpression_lf_primary body: " + context.classBody().GetText());
            }
            if (context.classBody() != null && context.classBody().GetText().Length > 0)
            {   
                _classHierarchyStack.Push(_signature);
                _className = "$ANON";
                _classHierarchyStack.Push("$ANON");
            }
        }

        // This method seems to be the one (and only one) point at which to listen to leaving the class body.
        public override void ExitClassBody([NotNull] Java8Parser.ClassBodyContext context)
        {
            base.ExitClassBody(context);
            if (_classHierarchyStack.Count > 0)
            {
                _className = _classHierarchyStack.Pop();
                if (_className == "$ANON")
                    // pop off the containing method name of the anonymous class
                    _className = _classHierarchyStack.Pop();
            }
        }

        /*
        public override void ExitClassBodyDeclaration([NotNull] Java8Parser.ClassBodyDeclarationContext context)
        {
            base.ExitClassBodyDeclaration(context);

        }
        */

        public override void ExitClassInstanceCreationExpression_lf_primary([NotNull] Java8Parser.ClassInstanceCreationExpression_lf_primaryContext context)
        {
            base.ExitClassInstanceCreationExpression_lf_primary(context);
            if (_classHierarchyStack.Count > 0)
                _className = _classHierarchyStack.Pop();
        }

        public override void EnterClassInstanceCreationExpression_lfno_primary([NotNull] Java8Parser.ClassInstanceCreationExpression_lfno_primaryContext context)
        {
            base.EnterClassInstanceCreationExpression_lfno_primary(context);
            if (MainWindow.IsDebugging)
            {
                WriteLine("EnterClassInstanceCreationExpression_lfno_primary: " + context.GetText());
                if (context.classBody() != null)
                    WriteLine("EnterClassInstanceCreationExpression_lfno_primary body: " + context.classBody().GetText());
            }
            if (context.classBody() != null && context.classBody().GetText().Length > 0)
            {
                // So far, this method, out of the three total EnterClassInstanceCreationExpression*() methods,
                // is the only one that seems to get called on encountering an anonymous class.  The similar code is
                // left in the other two methods just in case.        
                _classHierarchyStack.Push(_signature);
                _className = "$ANON";
                _classHierarchyStack.Push("$ANON");
            }
        }

        // This method got called multiple times when leaving an anonymous class, so instead of using this method, 
        // I'm using ExitClassBody, above, which seems to work for regular classes as well.
        /*
        public override void ExitClassInstanceCreationExpression_lfno_primary([NotNull] Java8Parser.ClassInstanceCreationExpression_lfno_primaryContext context)
        {
            //base.ExitClassInstanceCreationExpression_lfno_primary(context);
            // For some reason this method is called three times when leaving an anonymous class, so start
            // a counter, and only Pop() when the counter gets incremented twice.
            //if (_classHierarchyStack.Count > 0)
            //{
                //if (_anonymousMethodExitCounter == 3)
                //{
                //    _anonymousMethodExitCounter = 0;
                //    _className = _classHierarchyStack.Pop();
                //}
                //else if (_anonymousMethodExitCounter < 3)
                //    _anonymousMethodExitCounter++;
            //}
        }
        */

        public override void EnterNormalClassDeclaration([NotNull] Java8Parser.NormalClassDeclarationContext context)
        {
            base.EnterNormalClassDeclaration(context);
            if (MainWindow.IsDebugging)
                WriteLine("EnterNormalClassDeclaration: " + context.Identifier());
            //className = context.Identifier().ToString();
            if (context.Identifier().ToString() != _className)
            {
                _className = context.Identifier().ToString();
                _classHierarchyStack.Push(_className);
                _window.AddModule(_nestedPath, _path, _fileName, ClassHierarchyMinusInnermost() + _className);
            }
        }

        /*
        public override void ExitNormalClassDeclaration([NotNull] Java8Parser.NormalClassDeclarationContext context)
        {
            base.ExitNormalClassDeclaration(context);
            //if (_classHierarchyStack.Count > 0)
            //    _className = _classHierarchyStack.Pop();
        }
        */

        public override void EnterEnumDeclaration([NotNull] Java8Parser.EnumDeclarationContext context)
        {
            base.EnterEnumDeclaration(context);
            //The following produces a "redundant check before assignment" message, but that's o.k.
            if (context.Identifier().ToString() != _className)
            {
                _className = context.Identifier().ToString();
                _classHierarchyStack.Push(_className);
            }
        }

        public override void ExitEnumDeclaration([NotNull] Java8Parser.EnumDeclarationContext context)
        {
            base.EnterEnumDeclaration(context);
            if (_classHierarchyStack.Count > 0)
                _className = _classHierarchyStack.Pop();
        }

        /*
        public override void EnterMethodName([NotNull] Java8Parser.MethodNameContext context)
        {
            base.EnterMethodName(context);
            //if (MainWindow.IsDebugging)
            //    WriteLine("EnterMethodName: " + context.Identifier());
        }
        */

        public override void EnterConstructorDeclarator([NotNull] Java8Parser.ConstructorDeclaratorContext context)
        {
            base.EnterConstructorDeclarator(context);
            if (MainWindow.IsDebugging)
            {
                WriteLine("EnterConstructorDeclarator: " + context.simpleTypeName().Identifier());
            }

            //StringBuilder paramArgs = new StringBuilder(_className);
            //paramArgs.Append(".");
            StringBuilder paramArgs = new StringBuilder();
            paramArgs.Append(context.simpleTypeName().Identifier());
            bool isMoreThanOneArg = false;
            if (context.formalParameterList() != null && context.formalParameterList().formalParameters() != null
                && context.formalParameterList().formalParameters().formalParameter() != null)
            {
                isMoreThanOneArg = true;
                paramArgs.Append("(");
                foreach (
                    Java8Parser.FormalParameterContext param in
                        context.formalParameterList().formalParameters().formalParameter())
                {
                    paramArgs.Append(param.unannType().GetText() + ", ");
                }
            }
            if (context.formalParameterList() != null && context.formalParameterList().lastFormalParameter() != null
                && context.formalParameterList().lastFormalParameter().formalParameter() != null)
            {
                if (!isMoreThanOneArg)
                    paramArgs.Append("(");
                paramArgs.Append(
                    context.formalParameterList().lastFormalParameter().formalParameter().unannType().GetText());
                paramArgs.Append(")");
            }
            else if (!isMoreThanOneArg)
            {
                paramArgs.Append("()");
                //string str1 = context.formalParameterList().lastFormalParameter().unannType().GetText();
                //string str2 = context.formalParameterList().lastFormalParameter().variableDeclaratorId().GetText();
                //string str3 = context.formalParameterList().lastFormalParameter().variableModifier.
            }
            else
            {   // In this case, there are formalParameters, but not lastFormalParameter, so this
                // should uniquely identify the situation where we have a varargs parameter (variable
                // number of arguments) as the last parameter.
                // Some of this will be null in the case of varargs, so just insert "..." for varargs
                // (of unknown type at this point... there may be a way to get to the type, but I'm
                // not currently aware of it, and it could take some time to determine it.)
                paramArgs.Append("...)");
                //paramArgs.Append(context.formalParameterList().lastFormalParameter().formalParameter().unannType().GetText() + ")");
            }
            _signature = paramArgs.ToString();
            if (MainWindow.IsDebugging)
                WriteLine("EnterConstructorDeclarator: paramArgs:" + _signature);

            // More comprehensive checks probably ought to be done, to account for interfaces
            // that appear after a class or classes in a file.
            if (_className.Length > 0)
                _window.AddModule(_nestedPath, _path, _fileName, ClassHierarchy() + _signature);
                //_window.AddModule(_nestedPath, _path, _fileName, ClassHierarchyMinusInnermost() + _signature);

                /*
                if (!moduleList.Contains(new ModuleSubgrouped(nestedPath, path, fileName, newClassName)) && !moduleList.Contains(new Module(path, fileName, newClassName))
                    && (newClassName != null && newClassName.Length > 0))
                {

                    //queueList.Add(new Module(path, fileName, newClassName));
                    AddModule(nestedPath, path, fileName, newClassName);
                }
                */
        }

        public override void EnterMethodDeclarator([NotNull] Java8Parser.MethodDeclaratorContext context)
        {
            base.EnterMethodDeclarator(context);
            if (MainWindow.IsDebugging)
            {
                WriteLine("EnterMethodDeclarator: " + context.Identifier());
                //WriteLine("EnterMethodDeclarator.formalParameterList() = " + context.formalParameterList().formalParameters());
            }

            //StringBuilder paramArgs = new StringBuilder(_className);
            //paramArgs.Append(".");
            StringBuilder paramArgs = new StringBuilder();
            paramArgs.Append(context.Identifier());
            bool isMoreThanOneArg = false;
            if (context.formalParameterList() != null && context.formalParameterList().formalParameters() != null
                && context.formalParameterList().formalParameters().formalParameter() != null)
            {
                isMoreThanOneArg = true;
                paramArgs.Append("(");
                foreach (
                    Java8Parser.FormalParameterContext param in
                        context.formalParameterList().formalParameters().formalParameter())
                {
                    paramArgs.Append(param.unannType().GetText() + ", ");
                }
            }
            if (context.formalParameterList() != null && context.formalParameterList().lastFormalParameter() != null
                && context.formalParameterList().lastFormalParameter().formalParameter() != null)
            {
                if (!isMoreThanOneArg)
                    paramArgs.Append("(");
                paramArgs.Append(
                    context.formalParameterList().lastFormalParameter().formalParameter().unannType().GetText());
                paramArgs.Append(")");
            }
            else if (!isMoreThanOneArg)
            {
                paramArgs.Append("()");
                //string str1 = context.formalParameterList().lastFormalParameter().unannType().GetText();
                //string str2 = context.formalParameterList().lastFormalParameter().variableDeclaratorId().GetText();
                //string str3 = context.formalParameterList().lastFormalParameter().variableModifier.
            }
            else
            {   // In this case, there are formalParameters, but not lastFormalParameter, so this
                // should uniquely identify the situation where we have a varargs parameter (variable
                // number of arguments) as the last parameter.
                // Some of this will be null in the case of varargs, so just insert "..." for varargs
                // (of unknown type at this point... there may be a way to get to the type, but I'm
                // not currently aware of it, and it could take some time to determine it.)
                paramArgs.Append("...)");
                //paramArgs.Append(context.formalParameterList().lastFormalParameter().formalParameter().unannType().GetText() + ")");
            }
            _signature = paramArgs.ToString();
            if (MainWindow.IsDebugging)
                WriteLine("EnterMethodDeclarator: paramArgs:" + _signature);

            // More comprehensive checks probably ought to be done, to account for interfaces
            // that appear after a class or classes in a file.
            //if (className.Length > 0)
            //    _window.AddModule(_nestedPath, _path, _fileName, _signature);

            /*
            ITokenStream tokens = _parser.TokenStream;
            String type = "void";
            //if (ctx.type() != null)
            if (context.GetType() != null)
            {
                //type = tokens.getText(ctx.type().getSourceInterval());
                type = tokens.GetText(context.SourceInterval);
            }
            String args = String.Empty;
            if (context.formalParameterList() != null && context.formalParameterList().formalParameters() != null)
                args = tokens.GetText(context.formalParameterList().formalParameters());
            //args = context.formalParameterList().formalParameters();
            args += tokens.GetText(context.formalParameterList().lastFormalParameter());
            WriteLine("EnterMethodDeclarator: type:" + type + " Identifier():" + context.Identifier() + " args:" + args + ";");
            */

            /*
            if (context.formalParameterList() != null && context.formalParameterList().formalParameters() != null
                        && context.formalParameterList().formalParameters().formalParameter() != null)
            {
                string paramArgs = String.Empty;
                foreach (
                    Java8Parser.FormalParameterContext param in
                        context.formalParameterList().formalParameters().formalParameter())
                {
                    paramArgs += param.GetText() + " ";
                    List<IParseTree> subTree = new List<IParseTree>(param.children);
                    foreach (IParseTree subSubTree in subTree)
                    {
                        WriteLine("   IParseTree.ToStringTree() --> " + subSubTree.ToStringTree());
                        WriteLine("   IParseTree.ToString() --> " + subSubTree.ToString());
                    }
                }
                WriteLine("EnterMethoDeclarator: paramArgs:" + paramArgs);
            }
            */
        }

        /*
        public override void EnterMethodDeclaration([NotNull] Java8Parser.MethodDeclarationContext context)
        {
            base.EnterMethodDeclaration(context);
            if (MainWindow.IsDebugging)
            {
                // The following will just print out a bunch of numbers
                //WriteLine("EnterMethodDeclaration: " + context.methodBody().ToString());
                //WriteLine("EnterMethodDeclaration: " + context.methodBody().ToStringTree());

                WriteLine("EnterMethodDeclaration: " + context.GetText());
            }
        }
        */

        public override void EnterMethodBody([NotNull] Java8Parser.MethodBodyContext context)
        {
            // If context.GetText() != ";" then add the module
            base.EnterMethodBody(context);
            if (MainWindow.IsDebugging)
            {
                WriteLine("EnterMethodBody: " + context.GetText());

                // The following will just print out a bunch of numbers
                //WriteLine("EnterMethodBody: " + context.ToString());
                //WriteLine("EnterMethodbody: " + context.ToStringTree());
            }
            // The method forward declarations--such as those found in interfaces--
            // have bodies containing only semicolons, so only add modules to the 
            // moduleList where the method bodies don't consist of only a semicolon.
            // See the Java8.g4 grammar for methodBody.
            // This same method is not necessary for constructors.  Modules are generated
            // for constructors as they are encountered (see above, EnterConstructorDeclarator()).
            if (!context.GetText().StartsWith(";") && _className.Length > 0)
                _window.AddModule(_nestedPath, _path, _fileName, ClassHierarchy() + _signature);
        }

        public override void EnterStaticInitializer([NotNull] Java8Parser.StaticInitializerContext context)
        {
            base.EnterStaticInitializer(context);
            string classHierarchy = ClassHierarchy();
            _window.AddModule(_nestedPath, _path, _fileName, ClassHierarchy() + "static {}");
        }

        /// <summary>
        /// This method will cycle through the Stack _classHierarchyStack in reverse order
        /// to produce a list of outer to inner containing classes.
        /// </summary>
        /// <returns>String with a delimited (by ".") list of containing classes.</returns>
        private string ClassHierarchy()
        {   if (_classHierarchyStack.Count > 0)
            {
                StringBuilder classList = new StringBuilder();
                //string last = _classHierarchyStack.Peek();
                // Reverse iterators aren't that efficient... perhaps Stack is not the right structure for this.
                foreach (string s in _classHierarchyStack.Reverse())
                {
                    // Don't include the innermost containing class, since it is already built into _signature
                    //if (!s.Equals(last))
                        classList.Append(s + ".");
                }
                String str = classList.ToString();
                return str;
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// This is the same as the ClassHierarchy, except it doesn't include the innermost class.  At this point, might be
        /// a good idea to just rework this whole file.  NOTE:  Not used anymore.
        /// </summary>
        /// <returns></returns>
        private string ClassHierarchyMinusInnermost()
        {
            if (_classHierarchyStack.Count > 0)
            {
                StringBuilder classList = new StringBuilder();
                string last = _classHierarchyStack.Peek();
                // Reverse iterators aren't that efficient... perhaps Stack is not the right structure for this.
                foreach (string s in _classHierarchyStack.Reverse())
                {
                    // Don't include the innermost containing class, since it is already built into _signature
                    if (!s.Equals(last))
                        classList.Append(s + ".");
                }
                String str = classList.ToString();
                return str;
            }
            else
                return string.Empty;
        }

        /*
        public override void EnterFormalParameterList([NotNull] Java8Parser.FormalParameterListContext context)
        {
            base.EnterFormalParameterList(context);
            if (MainWindow.IsDebugging) ;
            {
                WriteLine("EnterFormatParameterList: " + context.GetText());
            }
        }
        */

        /*
        // Listen to matches of methodDeclaration
        public override void EnterMethodDeclaration([NotNull] Java8Parser.MethodDeclarationContext context)
        {
            ITokenStream tokens = _parser.TokenStream;
            String type = "void";
            //if (ctx.type() != null)
            if (context.GetType() != null)
            {
                //type = tokens.getText(ctx.type().getSourceInterval());
                type = tokens.GetText(context.SourceInterval);
            }
            //String args = tokens.GetText(context.);
            //String args = tokens.GetText(context.)
            WriteLine("EnterMethodDeclaration: \t" + type + " " + context.GetText());
        }
        */

    }
}
