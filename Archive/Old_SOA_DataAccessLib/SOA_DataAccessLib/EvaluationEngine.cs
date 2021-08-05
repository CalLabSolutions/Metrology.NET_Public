// Copyright 2017 Cal Lab Solutions Inc.
//
//This file is part of SOA_DataAccessLibrary.DLL, a component of Metrology.NET
//
//SOA_DataAccessLibrary.DLL is dual licensed software.
//
//For licensing for use in any product with lessor restrictions than the GPL, 
//  or for use in any proprietary licensed product, 
//  or for use in any or product publicly sold, 
//  or for use in any or product developed under contract for a another party,
//  contact Cal Lab Solutions Inc., to obtain an appropriate license
//
//      Cal Lab Solutions, Inc.
//      P.O. Box 111113
//      Aurora, CO 80042
//      Office: 303.317.6670
//      Fax: 303.317.5295
//      Email: sales@callabsolutions.com
//
//For commercial internal use only or for any strictly non-commercial use, 
//  SOA_DataAccessLibrary.DLL is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  SOA_DataAccessLibrary.DLL is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with SOA_DataAccessLibrary.DLL.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

internal class EvaluatorCore
{
    ConstantDictionary Constants = new ConstantDictionary();
    VariableDictionary Variables = new VariableDictionary();
    FunctionDictionary Functions = new FunctionDictionary();
    Parser parser;
    PostfixEvaluator EvaluationEngine;
    private Elements PostFix = null;

    public EvaluatorCore()
    {
        parser = new Parser(Variables, Functions, Constants);
        EvaluationEngine = new PostfixEvaluator(this, Variables, Functions);
        Reset();
    }

    public Elements Parse(string Expression)
    {
        Elements InFix = parser.Parse(Expression, true);
        PostFix = Compiler.Compile(InFix);
        return InFix;
    }

    public Object Execute(string Expression)
    {
        Elements InFix = parser.Parse(Expression, false);
        PostFix = Compiler.Compile(InFix);
        return (PostFix.Count > 0) ? EvaluationEngine.Evaluate(PostFix) : "";
    }

    public Object Execute()
    {
        return (PostFix.Count > 0) ? EvaluationEngine.Evaluate(PostFix) : "";
    }

    protected virtual void Initialize() { }

    public void Reset()
    {
        Variables.Clear();
        Functions.Clear();
        Constants.Clear();
        Initialize();
    }

    public void AddFunction(string FunctionName, Function F)
    {
        Functions.Add(FunctionName, F);
    }

    public void AddConstant(Constant C)
    {
        Constants.Add(C.text, C);
    }

    public IDictionary<String, Variable> GetVariables()
    {
        return Variables;
    }

    public object GetVariable(string VariableName)
    {
        object result = null;
        if (Variables.ContainsKey(VariableName))
            result = Variables[VariableName].value;
        return result;
    }

    public void SetVariable(string VariableName, object value)
    {
        string sval = value.ToString();
        long lval;
        double dblval;
        decimal decval;
        if (long.TryParse(sval, out lval))
            value = lval;
        else if (decimal.TryParse(sval, NumberStyles.Float, CultureInfo.InvariantCulture, out decval))
            value = decval;
        else if (double.TryParse(sval, out dblval))
            value = dblval;
        else
            throw new Exception("value is not a valid numeric");
        if (!Variables.ContainsKey(VariableName)) Variables.Add(VariableName, new Variable(VariableName));
        Variables[VariableName].value = value;
    }

}

internal enum OperatorID { OpenParen, CloseParen, Positive, Negation, Addition, Subtraction, Multiplication, Division, Exponentiation, Assignment, Comma, Semicolon }

internal class Operator
{
    protected int _priority;
    protected string text;  // useful for debugging
    protected OperatorID _id;
    public int priority { get { return _priority; } }
    public OperatorID id { get { return _id; } }
    public override string ToString() { return text; }
    public int NestingLevel = 0;
}

internal class LeftOperator : Operator { }

internal class ScriptError : Exception
{
    private Entity E;
    public Entity Entity { get { return E; } }
    Dictionary<string, object> Details = new Dictionary<string, object>();

    public override System.Collections.IDictionary Data
    {
        get
        {
            return Details;
        }
    }

    public ScriptError(Entity E, string Message)
        : base(Message)
    {
        this.E = E;
        Details["SourceOffset"] = E.SourceCodeIndex;
        Details["TokenLength"] = E.SourceCodeLength;
        Details["Text"] = E.ToString();

    }
}

internal class Comment
{
    public string text { get; set; }
    public Comment(string text) { this.text = text; }
    public override string ToString()
    {
        return text;
    }
}

internal class Unexpected
{
    public string text { get; set; }
    public Unexpected(string text) { this.text = text; }
    public override string ToString()
    {
        return text;
    }
}

internal class Variable
{
    public string text { get; set; }
    public object value { get; set; }
    public Variable(String text) { this.text = text; }
    public override string ToString()
    {
        return (value != null) ? value.ToString() : "unassigned";
    }
}

internal class Constant
{
    private string _text;
    private Object _value;
    public string text { get { return _text; } }
    public object value { get { return _value; } }
    public Constant(string Text, object Value) { _text = Text; _value = Value; }
    public override string ToString()
    {
        return value.ToString();
    }
}


internal class Terminator { }

internal abstract class Function
{
    public abstract Object Execute(EntityStack Args);
    public Entity self;
    protected object GetValue(Entity E)
    {
        object o = null;
        if (E.value is Variable) o = (E.value as Variable).value;
        else o = E.value;
        if (o is Entity) o = GetValue(o as Entity);
        return o;
    }
}

internal class Entity
{
    public object value;
    public int SourceCodeIndex;
    public int SourceCodeLength;
    public int BlockLevel = 0;

    protected Entity() { }

    public Entity(object value, int SourceCodeIndex, int SourceCodeLength)
        : base()
    {
        this.value = value;
        this.SourceCodeIndex = SourceCodeIndex;
        this.SourceCodeLength = SourceCodeLength;
    }

    public Entity(Entity entity, object newvalue)
        : base()
    {
        this.value = newvalue;
        this.SourceCodeIndex = entity.SourceCodeIndex;
        this.SourceCodeLength = entity.SourceCodeLength;
    }

    public Entity(Entity entity)
        : base()
    {
        this.value = entity.value;
        this.SourceCodeIndex = entity.SourceCodeIndex;
        this.SourceCodeLength = entity.SourceCodeLength;
    }

    public override string ToString()
    {
        return (value != null) ? value.ToString() : "Unassigned";
    }

}

internal class EntityStack : Stack<Entity> { }

internal class ConstantDictionary : Dictionary<String, Constant> { }
internal class VariableDictionary : Dictionary<String, Variable> { }
internal class FunctionDictionary : Dictionary<String, Function> { }

internal class OpenParenOperator : Operator { public OpenParenOperator() { _priority = -1; text = "("; _id = OperatorID.OpenParen; } }
internal class CloseParenOperator : Operator { public CloseParenOperator() { _priority = -1; text = ")"; _id = OperatorID.CloseParen; } }
internal class PositiveOperator : Operator { public PositiveOperator() { _priority = -1; text = "+"; _id = OperatorID.Positive; } }

internal class SemicolonSeperator : LeftOperator { public SemicolonSeperator() { _priority = 1; text = ";"; _id = OperatorID.Semicolon; } }

internal class AssignmentOperator : Operator { public AssignmentOperator() { _priority = 2; text = "="; _id = OperatorID.Assignment; } }

internal class AdditionOperator : Operator { public AdditionOperator() { _priority = 12; text = "+"; _id = OperatorID.Addition; } }
internal class SubtractionOperator : Operator { public SubtractionOperator() { _priority = 124; text = "-"; _id = OperatorID.Subtraction; } }

internal class MultiplicationOperator : Operator { public MultiplicationOperator() { _priority = 13; text = "*"; _id = OperatorID.Multiplication; } }
internal class DivisionOperator : Operator { public DivisionOperator() { _priority = 13; text = "/"; _id = OperatorID.Division; } }

internal class ExponentiationOperator : Operator { public ExponentiationOperator() { _priority = 14; text = "^"; _id = OperatorID.Exponentiation; } }

internal class NegationOperator : Operator { public NegationOperator() { _priority = 15; text = "-"; _id = OperatorID.Negation; } }

internal class CommaSeperator : LeftOperator { public CommaSeperator() { _priority = 4; text = ","; _id = OperatorID.Comma; } }


// An enhanced Queue of Entities; required to support looping and conditional blocks
internal class Elements : IEnumerable<Entity>
{
    List<Entity> list = new List<Entity>();

    public int HeadIndex { get; set; }

    public int Count { get { return list.Count - HeadIndex; } }

    public void Enqueue(Entity E)
    {
        list.Add(E);
    }

    public Entity Dequeue()
    {
        return list[HeadIndex++];
    }

    public Entity Peek()
    {
        return Peek(0);
    }

    public bool Advance()
    {
        int Index = HeadIndex;
        Index = Index + 1;
        HeadIndex = (Index < list.Count) ? Index : list.Count - 1;
        return Index == HeadIndex;
    }

    public bool Backup()
    {
        int Index = HeadIndex;
        Index = Index - 1;
        HeadIndex = (Index >= 0) ? Index : 0;
        return Index == HeadIndex;
    }

    public Entity Peek(int Index) // Index is relative to HeadIndex
    {
        int index = HeadIndex + Index;
        if ((index >= 0) && (index < list.Count))
            return list[HeadIndex + Index];
        else
            return new Entity(null, 0, 0);
    }

    public Entity LocateTo(int SourceCodePostion)
    {
        int I = list.Count() - 1;
        while ((I > 0) && (list[I].SourceCodeIndex > SourceCodePostion)) I--;
        HeadIndex = I;
        return list[I];
    }

    public override string ToString()
    {
        StringBuilder SB = new StringBuilder();
        for (int I = HeadIndex; (I < list.Count()) && (I - HeadIndex < 20); I++)
        {
            SB.AppendFormat("{0}, ", list[I].value.ToString());
        }
        return SB.ToString().TrimEnd(',', ' ');
    }

    public IEnumerator<Entity> GetEnumerator()
    {
        foreach (Entity E in list)
        {
            yield return E;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        foreach (Entity E in list)
        {
            yield return E;
        }
    }
}

internal class Parser
{
    ConstantDictionary Constants;
    VariableDictionary Variables;
    FunctionDictionary Functions;

    public Parser(VariableDictionary Variables, FunctionDictionary Functions, ConstantDictionary Constants)
    { this.Variables = Variables; this.Functions = Functions; this.Constants = Constants; }

    // Implements case insensitive regular expression pattern matching using a offset position in the input string.
    bool IsMatch(String pattern, String s, int offset, out int length, out string value)
    {
        Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
        Match m = r.Match(s, offset, s.Length - offset);
        length = m.Length;
        value = m.Value;
        return m.Success;
    }

    // returns whitespace if present in source string at offset position
    bool GetWhitespace(String s, int offset, out int length, out Entity entity)
    {
        entity = null;
        string text;
        if (IsMatch(@"^\s+", s, offset, out length, out text))
        {
            entity = new Entity(text as Object, offset, length);
            return true;
        }
        else return false;
    }

    // returns an integer if present in source string at offset position
    bool GetInteger(String s, int offset, out int length, out Entity entity)
    {
        entity = null;
        string text = s.Substring(offset);
        if (IsMatch(@"^(0+|[1-9]+[0-9]*)(?![e\.])", s, offset, out length, out text))
        {
            entity = new Entity(int.Parse(text) as Object, offset, length);
            return true;
        }
        else return false;
    }

    // returns a Floating Point number if present in source string at offset position
    bool GetFloat(String s, int offset, out int length, out Entity entity)
    {
        entity = null;
        string text1 = s.Substring(offset);
        string text;
        if (IsMatch(@"^((0+|[1-9]+[0-9]*)+\.[0-9]*|((0+|[1-9]+[0-9]*)+\.[0-9]*|[1-9]+[0-9]*)e[\+\-](0+|[1-9]+[0-9]*))", s, offset, out length, out text))
        {
            decimal dval;
            if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out dval))
            {
                entity = new Entity(dval, offset, length);
                return true;
            }
            else return false;
        }
        else return false;
    }

    // returns a simple Operator if present in source string at offset position
    bool GetOperator(ref Boolean BinaryMode, String s, int offset, out int length, out Entity entity)
    {
        entity = null;
        object oper = null;
        string text;
        string RegexPattern = @"^([-+*/^(),;=])";
        if (IsMatch(RegexPattern, s, offset, out length, out text))
        {
            switch (text.ToLower())
            {
                case "^": oper = new ExponentiationOperator() as Object; break;
                case "(": oper = new OpenParenOperator() as Object; break;
                case ")": oper = new CloseParenOperator() as Object; break;
                case "*": oper = new MultiplicationOperator() as Object; break;
                case "/": oper = new DivisionOperator() as Object; break;
                case "+": oper = BinaryMode ? new AdditionOperator() as Object : new PositiveOperator() as Object; break;
                case "-": oper = BinaryMode ? new SubtractionOperator() as Object : new NegationOperator() as Object; break;
                case "=": oper = new AssignmentOperator() as Object; break;
                case ",": oper = new CommaSeperator() as Object; break;
                case ";": oper = new SemicolonSeperator() as Object; break;
            }
            BinaryMode = false;
            entity = new Entity(oper, offset, length);
            return true;
        }
        else return false;
    }

    // returns a Function Operator if present in source string at offset position
    bool GetFunction(String s, int offset, out int length, out Entity entity, bool IgnoreErrors)
    {
        entity = null;
        String text;
        bool result = false;
        if (IsMatch(@"^[_a-z][_a-z0-9]*\(", s, offset, out length, out text)) // Matches any "C" syntax function name
        {
            text = text.TrimEnd('('); // the '(" is not part of the Function Name
            length = length - 1;
            string S;
            int L;
            if (IsMatch(@"[a-z]", text, 0, out L, out S))// additional syntax restriction to require at least one letter in the name.
            {
                foreach (string Key in Functions.Keys) // allows case insensitive match of function names
                {
                    if (Key.ToLower() == text.ToLower())
                    {
                        entity = new Entity(Functions[Key], offset, length);
                        result = true;
                        break;
                    }
                }
                if ((!result) && (!IgnoreErrors)) throw new ScriptError(new Entity(text, offset, length), "Unrecognized Function Name");
            }
        }
        return result;
    }

    // returns a Constant or Variable if present in source string at offset position
    bool GetConstantOrVariable(String s, int offset, out int length, out Entity entity, out Entity IndexOp)
    {
        entity = null;
        object o = null;
        IndexOp = null;
        String text;
        if (IsMatch(@"^[_a-z][_a-z0-9]*", s, offset, out length, out text)) // Matches any "C" syntax variable name
        {
            string S;
            int L;
            if (IsMatch(@"[a-z]", text, 0, out L, out S))// additional syntax restriction to require at least one letter in the name.
            {
                foreach (string Key in Constants.Keys) // allows case insensitive match of constant names
                {
                    if (Key.ToLower() == text.ToLower())
                    {
                        o = Constants[Key].value;
                        break;
                    }
                }
                foreach (string Key in Variables.Keys) // allows case insensitive match of variable names
                {
                    if (Key.ToLower() == text.ToLower())
                    {
                        o = Variables[Key];
                        break;
                    }
                }
                if (o == null)
                {
                    Variables.Add(text, new Variable(text));
                    o = Variables[text];
                }
                entity = new Entity(o, offset, length);
                return true;
            }
        }
        return false;
    }

    // returns the objects represented in the input string in Infix Order
    public Elements Parse(string input, bool IgnoreErrors)
    {
        int offset = 0;
        int length;
        Entity entity;
        Entity IndexOp = null;
        Elements elements = new Elements();
        Boolean BinaryMode = false;
        Boolean unidentified = true;
        if (input != null) while (offset < input.Length)
            {
                if (GetWhitespace(input, offset, out length, out entity))
                {
                    offset += length;
                    unidentified = false;
                }
                else if (GetOperator(ref BinaryMode, input, offset, out length, out entity))
                {
                    offset += length;
                    if (!(entity.value is PositiveOperator)) elements.Enqueue(entity);
                    BinaryMode = entity.value is CloseParenOperator;
                    unidentified = false;
                }
                else if (GetFloat(input, offset, out length, out entity)
                    || GetInteger(input, offset, out length, out entity)
                    || GetFunction(input, offset, out length, out entity, IgnoreErrors)
                    || GetConstantOrVariable(input, offset, out length, out entity, out IndexOp))
                {
                    offset += length;
                    elements.Enqueue(entity);
                    if (IndexOp != null)
                    {
                        elements.Enqueue(IndexOp);
                        IndexOp = null;
                    }
                    BinaryMode = true;
                    unidentified = false;
                }
                else if (IgnoreErrors)
                {
                    if (!unidentified)
                        elements.Enqueue(new Entity(new Unexpected(input[offset].ToString()), offset, 1));
                    else
                        elements.Peek().SourceCodeLength = elements.Peek().SourceCodeLength + 1;
                    unidentified = true;
                    offset++;
                }
                else
                    throw new ScriptError(new Entity(input[offset], offset, 1), "Unexpected Character");
            }
        return elements;
    }
}

internal class Compiler
{
    // Converts InFix ordered Elements to PostFix Ordered Elements
    // look on-line for articles on the "Shunting Yard Algorithm"
    // in order to understand what this routine is doing.
    public static Elements Compile(Elements Infix)
    {
        Elements PostFix = new Elements();
        EntityStack Operators = new EntityStack();
        Entity e = null;
        EntityStack Parens = new EntityStack();
        EntityStack IfsAndLoops = new EntityStack();
        EntityStack Ifs = new EntityStack();
        Boolean ValueFlag = false;
        object O;
        {
            foreach (Entity E in Infix)
            {
                E.BlockLevel = IfsAndLoops.Count;
                O = E.value;
                if (O is Unexpected) { }
                else if (O is Comment) { }
                else if (O is Function)
                {
                    PostFix.Enqueue(new Entity(new Terminator(), -1, -1));  // Terminators don't exist in source code
                    Operators.Push(E);
                }
                else if (O is Operator)
                {
                    ValueFlag = false;
                    if (O is OpenParenOperator)
                    {
                        Parens.Push(E);
                        Operators.Push(E);
                    }
                    else if (O is CloseParenOperator)
                    {
                        if ((Parens.Count == 0) || !(Parens.Pop().value is OpenParenOperator))
                            throw new ScriptError(E, "Missing or Misplaced \"(\"");
                        while ((Operators.Count > 0) && (!(Operators.Peek().value is OpenParenOperator)))
                        {
                            e = Operators.Pop();
                            PostFix.Enqueue(e);
                        }
                        if (Operators.Count > 0) Operators.Pop();  // Pop the OpenParen
                        else
                            throw new ScriptError(E, "Missing Open '('");
                        ValueFlag = true;
                    }
                    else if (O is LeftOperator)
                    {
                        while ((Operators.Count > 0) && ((Operators.Peek().value is Function) || ((Operators.Peek().value as Operator).priority > (O as Operator).priority)))
                        {
                            e = Operators.Pop();
                            PostFix.Enqueue(e);
                        }
                        PostFix.Enqueue(E);
                    }
                    else
                    {
                        while ((Operators.Count > 0) && ((Operators.Peek().value is Function) || ((Operators.Peek().value as Operator).priority >= (O as Operator).priority)))
                        {
                            e = Operators.Pop();
                            PostFix.Enqueue(e);
                        }
                        Operators.Push(E);
                    }
                    if ((O is SemicolonSeperator) || (O is AssignmentOperator))
                    {
                        if (Parens.Count > 0)
                        {
                            Entity E1 = Parens.Pop();
                        }
                    }
                }
                else
                {
                    if (ValueFlag)
                        throw new ScriptError(E, "Missing separator or operator");
                    PostFix.Enqueue(E);
                    ValueFlag = true;
                }
            }
            while (Operators.Count > 0) PostFix.Enqueue(Operators.Pop());
        }
        return PostFix;
    }
}

internal class PostfixEvaluator
{
    VariableDictionary Variables;
    FunctionDictionary Functions;
    EvaluatorCore ScriptEngine;

    public PostfixEvaluator(EvaluatorCore ScriptEngine, VariableDictionary Variables, FunctionDictionary Functions)
    { this.ScriptEngine = ScriptEngine; this.Variables = Variables; this.Functions = Functions; }

    public object Evaluate(Elements Postfix)
    {
        Entity E = null;
        EntityStack ValueStack = new EntityStack();
        EntityStack Arguments = new EntityStack();
        EntityStack ReversingStack = new EntityStack();
        Object O = null;
        bool NewStatement;
        while (Postfix.Count > 0)
        {
            E = Postfix.Dequeue();
            O = E.value;
            NewStatement = (O is SemicolonSeperator) || (Postfix.Count == 0);
            if (O is Function)
            {
                Arguments.Clear();
                if (ValueStack.Count > 0)
                {
                    do { Arguments.Push(new Entity(E, PopValueOnStack(ValueStack))); } while (!(Arguments.Peek().value is Terminator) && (ValueStack.Count > 0));
                    Arguments.Pop();
                }
                (O as Function).self = E;
                O = (O as Function).Execute(Arguments);
                if (O != null) ValueStack.Push(new Entity(E, O));
            }
            else if (O is Operator)
            {
                switch ((O as Operator).id)
                {
                    case OperatorID.OpenParen:
                        break;
                    case OperatorID.Comma:
                        break;
                    case OperatorID.Semicolon:
                        break;
                    case OperatorID.Negation:
                        ValueStack.Push(new Entity(E, Negate(PopValueOnStack(ValueStack))));
                        if (ValueStack.Peek().value == null) throw new ScriptError(E, "Operand of Improper Type");
                        break;
                    case OperatorID.Addition:
                        ValueStack.Push(new Entity(E, Add(PopValueOnStack(ValueStack), PopValueOnStack(ValueStack))));
                        if (ValueStack.Peek().value == null) throw new ScriptError(E, "One or More Operands of Improper Type");
                        break;
                    case OperatorID.Subtraction:
                        ValueStack.Push(new Entity(E, Subtract(PopValueOnStack(ValueStack), PopValueOnStack(ValueStack))));
                        if (ValueStack.Peek().value == null) throw new ScriptError(E, "One or More Operands of Improper Type");
                        break;
                    case OperatorID.Multiplication:
                        ValueStack.Push(new Entity(E, Multiply(PopValueOnStack(ValueStack), PopValueOnStack(ValueStack))));
                        if (ValueStack.Peek().value == null) throw new ScriptError(E, "One or More Operands of Improper Type");
                        break;
                    case OperatorID.Division:
                        ValueStack.Push(new Entity(E, Divide(PopValueOnStack(ValueStack), PopValueOnStack(ValueStack))));
                        if (ValueStack.Peek().value == null) throw new ScriptError(E, "One or More Operands of Improper Type");
                        break;
                    case OperatorID.Exponentiation:
                        ValueStack.Push(new Entity(E, Raise(PopValueOnStack(ValueStack), PopValueOnStack(ValueStack))));
                        if (ValueStack.Peek().value == null) throw new ScriptError(E, "One or More Operands of Improper Type");
                        break; ;
                    case OperatorID.Assignment:
                        if (ValueStack.Count == 2) // example "A = 1"; assigns 1 to A
                        {
                            Assign(ValueStack.Pop(), ValueStack.Pop(), Variables);
                        }
                        else
                        {
                            if (ValueStack.Count == 1) // example "= 1"; the expression result is 1
                                ValueStack.Push(new Entity(E, Assign(ValueStack.Pop(), null, Variables)));
                            if ((ValueStack.Count == 0) || ((ValueStack.Count > 0) && (ValueStack.Peek().value == null)))
                                throw new ScriptError(E, "One or More Operands of Improper Type");
                            if (ValueStack.Count > 2)
                                throw new ScriptError(E, "Too Many values on ValueStack.  Something went terribly wrong!");
                        }
                        break;
                }
            }
            else
            {
                if (O is Constant) O = (O as Constant).value;
                else if ((O is Variable) && Variables.ContainsKey((O as Variable).text)) O = Variables[(O as Variable).text];
                ValueStack.Push(new Entity(E, O));
                if (ValueStack.Peek() == null) throw new ScriptError(E, "Improper use of Unassigned Variable");
            }
        }
        return (ValueStack.Count == 0) ? null : GetValue(ValueStack.Pop());
    }

    object GetValue(Entity E)
    {
        object o = null;
        if (E.value is Variable) o = (E.value as Variable).value;
        else o = E.value;
        if (o is Entity) o = GetValue(o as Entity);
        return o;
    }

    object PopValueOnStack(EntityStack ValueStack)
    {
        if (ValueStack.Count > 0)
            return GetValue(ValueStack.Pop());
        else
            return null;
    }

    object PeekValueOnStack(EntityStack ValueStack)
    {
        if (ValueStack.Count > 0)
            return GetValue(ValueStack.Peek());
        else
            return null;
    }

    object Negate(object o)
    {
        if (o is int) o = -(long)o;
        else if (o is decimal) o = -(decimal)o;
        else if (o is double) o = -(double)o;
        else o = null;
        return o;
    }

    object Add(object o1, object o2)
    {
        object o;
        if ((o1 is long) && (o2 is long)) o = (long)o2 + (long)(o1);
        else if ((o1 is decimal) && (o2 is decimal)) o = (decimal)o2 + (decimal)o1;
        else if ((o1 is long) && (o2 is decimal)) o = (decimal)o2 + (long)o1;
        else if ((o1 is decimal) && (o2 is int)) o = (long)o2 + (decimal)o1;
        else if ((o1 is double) && (o2 is double)) o = (double)o2 + (double)o1;
        else if ((o1 is long) && (o2 is double)) o = (double)o2 + (long)o1;
        else if ((o1 is double) && (o2 is int)) o = (long)o2 + (double)o1;
        else if ((o1 is decimal) && (o2 is double)) o = (double)o2 + (double)o1;
        else if ((o1 is double) && (o2 is decimal)) o = (double)o2 + (double)o1;
        else
            o = null;
        return o;
    }

    object Subtract(object o1, object o2)
    {
        object o;
        if ((o1 is long) && (o2 is long)) o = (long)o2 - (long)(o1);
        else if ((o1 is decimal) && (o2 is decimal)) o = (decimal)o2 - (decimal)o1;
        else if ((o1 is long) && (o2 is decimal)) o = (decimal)o2 - (long)o1;
        else if ((o1 is decimal) && (o2 is int)) o = (long)o2 - (decimal)o1;
        else if ((o1 is double) && (o2 is double)) o = (double)o2 - (double)o1;
        else if ((o1 is long) && (o2 is double)) o = (double)o2 - (long)o1;
        else if ((o1 is double) && (o2 is int)) o = (long)o2 - (double)o1;
        else if ((o1 is decimal) && (o2 is double)) o = (double)o2 - (double)o1;
        else if ((o1 is double) && (o2 is decimal)) o = (double)o2 - (double)o1;
        else
            o = null;
        return o;
    }

    object Multiply(object o1, object o2)
    {
        object o;
        if ((o1 is long) && (o2 is long)) o = (long)o2 * (long)(o1);
        else if ((o1 is decimal) && (o2 is decimal)) o = (decimal)o2 * (decimal)o1;
        else if ((o1 is long) && (o2 is decimal)) o = (decimal)o2 * (long)o1;
        else if ((o1 is decimal) && (o2 is int)) o = (long)o2 * (decimal)o1;
        else if ((o1 is double) && (o2 is double)) o = (double)o2 * (double)o1;
        else if ((o1 is long) && (o2 is double)) o = (double)o2 * (long)o1;
        else if ((o1 is double) && (o2 is int)) o = (long)o2 * (double)o1;
        else if ((o1 is decimal) && (o2 is double)) o = (double)o2 * (double)o1;
        else if ((o1 is double) && (o2 is decimal)) o = (double)o2 * (double)o1;
        else
            o = null;
        return o;
    }

    object Divide(object o1, object o2)
    {
        object o;
        if ((o1 is long) && (o2 is long)) o = (long)o2 / (long)(o1);
        else if ((o1 is decimal) && (o2 is decimal)) o = (decimal)o2 / (decimal)o1;
        else if ((o1 is long) && (o2 is decimal)) o = (decimal)o2 / (long)o1;
        else if ((o1 is decimal) && (o2 is int)) o = (long)o2 / (decimal)o1;
        else if ((o1 is double) && (o2 is double)) o = (double)o2 / (double)o1;
        else if ((o1 is long) && (o2 is double)) o = (double)o2 / (long)o1;
        else if ((o1 is double) && (o2 is int)) o = (long)o2 / (double)o1;
        else if ((o1 is decimal) && (o2 is double)) o = (double)o2 / (double)o1;
        else if ((o1 is double) && (o2 is decimal)) o = (double)o2 / (double)o1;
        else
            o = null;
        return o;

    }

    object Raise(object o1, object o2)
    {
        object o;
        if ((o1 is long) && (o2 is long)) o = Math.Pow((long)o2, (long)(o1));
        else if ((o1 is decimal) && (o2 is decimal)) o = DecimalMath.Pow((decimal)o2, (decimal)o1);
        else if ((o1 is long) && (o2 is decimal)) o = DecimalMath.Pow((decimal)o2, (long)o1);
        else if ((o1 is decimal) && (o2 is int)) o = DecimalMath.Pow((long)o2, (decimal)o1);
        else if ((o1 is double) && (o2 is double)) o = Math.Pow((double)o2, (double)o1);
        else if ((o1 is long) && (o2 is double)) o = Math.Pow((double)o2, (long)o1);
        else if ((o1 is double) && (o2 is int)) o = Math.Pow((long)o2, (double)o1);
        else if ((o1 is decimal) && (o2 is double)) o = Math.Pow((double)o2, (double)o1);
        else if ((o1 is double) && (o2 is decimal)) o = Math.Pow((double)o2, (double)o1);
        else
            o = null;
        return o;
    }

    object Assign(Entity e1, Entity e2, VariableDictionary Variables)
    {
        object o = null; // example 1 = 2; assignment doesn't make sense
        if (e2 != null)
        {
            if (e2.value is Variable) // example "A = B"; assigns B to A
            {
                if (!Variables.ContainsKey((e2.value as Variable).text)) Variables.Add((e2.value as Variable).text, (e2.value as Variable));
                (e2.value as Variable).value = GetValue(e1);
                o = GetValue(e1);
            }
            else
                throw new ScriptError(e2, "Can't make sense of this assignment");
        }
        else
        {
            if (e1.value is Variable) // example "= A"; the expression result is value of A
                o = GetValue(e1);
            else
                throw new ScriptError(e1, "Can't make sense of this assignment");
        }
        return o;
    }


}

public class DecimalMath
{
    // Adjust this to modify the precision
    const int ITERATIONS = 27;
    static decimal log10 = 0.0m;

    // power series
    public static decimal Exp(decimal power)
    {
        int iteration = ITERATIONS;
        decimal result = 1;
        while (iteration > 0)
        {
            var fatorial = Factorial(iteration);
            result += Pow(power, iteration) / fatorial;
            iteration--;
        }
        return result;
    }

    // natural logarithm series
    public static decimal LogN(decimal number)
    {
        decimal aux = (number - 1);
        decimal result = 0;
        int iteration = ITERATIONS;
        while (iteration > 0)
        {
            result += Pow(aux, iteration) / iteration;
            iteration--;
        }
        return result;
    }

    // logarithm base 10
    public static decimal Log10(decimal number)
    {
        if (log10 == 0.0m) log10 = LogN(10.0m);
        return LogN(number)/log10;
    }

    public static decimal Factorial(long number)
    {
        decimal f = 1.0m;
        for (long n = number; n > 1; n--)
        {
            f *= n;
        }
        return f;
    }

    public static decimal Pow(decimal value, long number)
    {
        decimal f = 1.0m;
        for (long n = number; n > 1; n--)
        {
            f *= value;
        }
        return f;
    }

    public static decimal Pow(decimal baseValue, decimal exponent)
    {
        return Exp(exponent * LogN(baseValue));
    }

}

internal class EvaluationEngine : EvaluatorCore
{
    protected override void Initialize()
    {
        base.Initialize();
        AddFunction("Abs", new Abs());
        AddFunction("Average", new Average());
        AddFunction("Max", new Max());
        AddFunction("Min", new Min());
        AddFunction("Round", new Round());
        AddFunction("StdDev", new StdDev());
        AddFunction("Sum", new Sum());
        AddFunction("Truncate", new Truncate());
        AddConstant(new Constant("PI", (decimal)3.141592653589793238462643383279502884));
    }
}

// object result = Abs((int or decimal) Arg)
internal class Abs : Function
{
    public override string ToString() { return "Abs()"; }
    public override Object Execute(EntityStack Args)
    {
        Entity Arg;
        object value, result = null;
        if (Args.Count == 1)
        {
            Arg = Args.Pop();
            value = GetValue(Arg);
            if (value is int)
                result = Math.Abs((int)value);
            else if (value is decimal)
                result = Math.Abs((decimal)value);
            else
                throw new ScriptError(Arg, "Argument must be a numeric type");
        }
        else
            throw new ScriptError(self, "Abs() Requires one numeric argument");
        return result;
    }
}

// decimal result = Average( (int or decimal) Arg1, (int or decimal) Arg2,...)
internal class Average : Function
{
    public override string ToString() { return "Average()"; }
    public override Object Execute(EntityStack Args)
    {
        List<decimal> Values = new List<decimal>();
        decimal sum = 0.0m, average;
        object result = null;
        while (Args.Count > 0)
        {
            Entity E = Args.Pop();
            object o = GetValue(E);
            if ((o is int)) Values.Add((int)o);
            else if ((o is decimal)) Values.Add((decimal)o);
            else
                throw new ScriptError(E, "Encountered a non-numeric Argument");
        }
        if (Values.Count > 0)
        {
            foreach (decimal v in Values) sum += v;
            average = sum / Values.Count;
            result = average;
        }
        else
            throw new ScriptError(self, "Improper Argument List");
        return result;
    }
}

// decimal result = max( (int or decimal) Arg1, (int or decimal) Arg2,...)
internal class Max : Function
{
    public override string ToString() { return "Max()"; }
    public override Object Execute(EntityStack Args)
    {
        List<decimal> Values = new List<decimal>();
        decimal max = -decimal.MaxValue;
        object result = null;
        while (Args.Count > 0)
        {
            Entity E = Args.Pop();
            object o = GetValue(E);
            if ((o is int)) Values.Add((int)o);
            else if ((o is decimal)) Values.Add((decimal)o);
            else
                throw new ScriptError(E, "Encountered a non-numeric Argument");
        }
        if (Values.Count > 0)
        {
            foreach (decimal d in Values) max = Math.Max(max, d);
            result = max;
        }
        else
            throw new ScriptError(self, "Improper Argument List");
        return result;
    }
}

// decimal result = min( (int or decimal) Arg1, (int or decimal) Arg2,...)
internal class Min : Function
{
    public override string ToString() { return "Min()"; }
    public override Object Execute(EntityStack Args)
    {
        List<decimal> Values = new List<decimal>();
        decimal min = decimal.MaxValue;
        object result = null;
        while (Args.Count > 0)
        {
            Entity E = Args.Pop();
            object o = GetValue(E);
            if ((o is int)) Values.Add((int)o);
            else if ((o is decimal)) Values.Add((decimal)o);
            else
                throw new ScriptError(E, "Encountered a non-numeric Argument");
        }
        if (Values.Count > 0)
        {
            foreach (decimal d in Values) min = Math.Min(min, d);
            result = min;
        }
        else
            throw new ScriptError(self, "Improper Argument List");
        return result;
    }
}

// object result = Round((decimal) Arg1, (optional int) Arg2)
internal class Round : Function
{
    public override string ToString() { return "Round()"; }
    public override Object Execute(EntityStack Args)
    {
        Entity Arg1, Arg2;
        object value1, value2, result = null;
        if (Args.Count == 1)
        {
            Arg1 = Args.Pop();
            value1 = GetValue(Arg1);
            if (value1 is decimal)
                result = (int)Math.Round((decimal)value1);
            else
                throw new ScriptError(Arg1, "Argument must be a floating point numeric type");
        }
        else if (Args.Count == 2)
        {
            Arg1 = Args.Pop(); Arg2 = Args.Pop();
            value1 = GetValue(Arg1); value2 = GetValue(Arg2);
            if (!(value1 is decimal)) throw new ScriptError(Arg1, "First Argument must be a floating point numeric type");
            if (!(value2 is int)) throw new ScriptError(Arg1, "Second Argument must be an integer numeric type");
            result = Math.Round((decimal)value1, (int)value2);
        }
        else
            throw new ScriptError(self, "Improper Argument List");
        return result;
    }
}

// decimal result = sum( (int or decimal) Arg1, (int or decimal) Arg2,...)
internal class Sum : Function
{
    public override string ToString() { return "Sum()"; }
    public override Object Execute(EntityStack Args)
    {
        List<decimal> Values = new List<decimal>();
        decimal sum = 0.0m;
        object result = null;
        while (Args.Count > 0)
        {
            Entity E = Args.Pop();
            object o = GetValue(E);
            if ((o is int)) Values.Add((int)o);
            else if ((o is decimal)) Values.Add((decimal)o);
            else
                throw new ScriptError(E, "Encountered a non-numeric Argument");
        }
        if (Values.Count > 0)
        {
            foreach (decimal d in Values) sum = sum + d;
            result = sum;
        }
        else
            throw new ScriptError(self, "Improper Argument List");
        return result;
    }
}

// decimal result = StdDev((int or decimal) Arg1, (int or decimal) Arg2,...)
internal class StdDev : Function
{
    public override string ToString() { return "StdDev()"; }
    public override Object Execute(EntityStack Args)
    {
        List<decimal> Values = new List<decimal>();
        decimal sum = 0.0m, average, variance = 0.0m;
        object result = null;

        while (Args.Count > 0)
        {
            Entity E = Args.Pop();
            object o = GetValue(E);
            if ((o is int)) Values.Add((int)o);
            else if ((o is decimal)) Values.Add((decimal)o);
            else
                throw new ScriptError(E, "Encountered a non-numeric Argument");
        }
        if (Values.Count > 0)
        {
            foreach (decimal v in Values) sum += v;
            average = sum / Values.Count;
            foreach (decimal v in Values) variance += (v - average) * (v - average);
            result = Sqrt(variance / (Values.Count - 1)); 
        }
        else
            throw new ScriptError(self, "Improper Argument List");
        return result;
    }

    /// <summary>
    /// this is a replacement for Math.sqrt or Math.pow that only work with the precision of double 
    /// </summary>
    public static decimal Sqrt(decimal x, decimal? guess = null)
    {
        var ourGuess = guess.GetValueOrDefault(x / 2m);
        var result = x / ourGuess;
        var average = (ourGuess + result) / 2m;

        if (average == ourGuess) // This checks for the maximum precision possible with a decimal.
            return average;
        else
            return Sqrt(x, average);
    }
}

// object result = Truncate((decimal) Arg)
internal class Truncate : Function
{
    public override string ToString() { return "Trunc()"; }
    public override Object Execute(EntityStack Args)
    {
        Entity Arg;
        object value, result = null;
        if (Args.Count == 1)
        {
            Arg = Args.Pop();
            value = GetValue(Arg);
            if (value is decimal)
                result = (int)Math.Truncate((decimal)value);
            else
                throw new ScriptError(Arg, "Argument must be a floating point numeric type");
        }
        else
            throw new ScriptError(self, "Truncate() Requires one floating point numeric argument");
        return result;
    }
}
