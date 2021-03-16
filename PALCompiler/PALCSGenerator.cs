using System.Text;

namespace PALCompiler
{
    /// <summary>
    /// Generates C# source from the syntax tree that has undergone semantic
    /// analysis
    /// </summary>
    partial class PALCSGenerator
    {
        public SyntaxNode syntax_tree;

        internal PALCSGenerator(SyntaxNode syntax_tree) { this.syntax_tree = syntax_tree; }

        internal string generate()
        {
            var artifact = new StringBuilder();

            artifact.Append(Generators.generateProgram(syntax_tree));

            return artifact.ToString();
        }
    }
}
