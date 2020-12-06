
using Microsoft.CSharp;

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Origine
{
    public class CodeMemberInfo
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public Type Type { get; set; }
        public object Value { get; set; }
    }

    public static class CodeGenerateUtility
    {
        public static string GenerateCSharpCode(string path,
            string ns,
            string className,
            string fileName,
            Func<List<CodeTypeMember>> action)
        {
            var _compileunit = new CodeCompileUnit();
            var @namespace = new CodeNamespace(ns);
            var @class = new CodeTypeDeclaration(className);
            @class.IsPartial = true;
            @class.TypeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;
            @namespace.Types.Add(@class);

            foreach (var member in action.Invoke())
            {
                @class.Members.Add(member);
            }

            _compileunit.Namespaces.Add(@namespace);

            var provider = new CSharpCodeProvider();
            var sourceFile = $"{Path.Combine(path, fileName ?? className)}{(provider.FileExtension[0] == '.' ? ' ' : '.')}{provider.FileExtension}";

            using (StreamWriter sw = new StreamWriter(sourceFile, false))
            {
                var tw = new IndentedTextWriter(sw, "    ");
                provider.GenerateCodeFromCompileUnit(_compileunit, tw, new CodeGeneratorOptions());
                tw.Close();
            }

            return sourceFile;
        }
    }

}