using System;
using Antlr4.Runtime;

namespace ModuleJogger
{

    public class DescriptiveErrorListener : BaseErrorListener
    {
        public static DescriptiveErrorListener INSTANCE = new DescriptiveErrorListener();

        // in Java:
        //public override SyntaxError(Recognizer<?, ?> recognizer, Object offendingSymbol,
        //                    int line, int charPositionInLine,
        //                    String msg, RecognitionException e)

        public override void SyntaxError(Antlr4.Runtime.IRecognizer recognizer, Antlr4.Runtime.IToken offendingSymbol,
                    int line, int charPositionInLine, String msg, Antlr4.Runtime.RecognitionException e)
        {
            string sourceName = recognizer.InputStream.SourceName;
            if (!string.IsNullOrEmpty(sourceName))
            {
                //sourceName = String.format("%s:%d:%d: ", sourceName, line, charPositionInLine);
                sourceName = sourceName + ":line " + line + ":char " + charPositionInLine + ":msg " + msg;
            }
            Console.Error.WriteLine(sourceName + "line " + line + ":" + charPositionInLine + " " + msg);

            // If it is desired for processing to continue, it may be necessary to not throw the exception (or
            // any exception) below.
            // Or OperationCanceledException:
            //throw new InvalidOperationException(sourceName);
        }
    }

    /*
    //https://github.com/antlr/antlr4/blob/master/runtime/Java/src/org/antlr/v4/runtime/misc/ParseCancellationException.java
    /
    // * This exception is thrown to cancel a parsing operation. This exception does
    // * not extend {@link RecognitionException}, allowing it to bypass the standard
    // * error recovery mechanisms. {@link BailErrorStrategy} throws this exception in
    // * response to a parse error.
    // *
    // * @author Sam Harwell
    //
    public class ParseCancellationException extends CancellationException
    {

        public ParseCancellationException() {
        }

        public ParseCancellationException(String message) {
            super(message);
        }

        public ParseCancellationException(Throwable cause) {
            initCause(cause);
        }

        public ParseCancellationException(String message, Throwable cause) {
            super(message);
            initCause(cause);
        }
    }
    */

}
