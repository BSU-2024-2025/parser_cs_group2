namespace CodeExecuter.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    namespace Expression
    {
        public class Expression
        {
            static void Main() { }

            private static Dictionary<string, double> variables = new Dictionary<string, double>();

            public static string ParseExpression(string expression)
            {
                expression = Regex.Replace(expression, "//.*$", string.Empty, RegexOptions.Multiline);
                expression = Regex.Replace(expression, @"\s+", "");
                expression = Regex.Replace(expression, " ", "");
                return expression.Trim();
            }
            public static bool IsValidExpression(string expression)
            {
                if (string.IsNullOrEmpty(expression)) return false;

                Stack<char> stack = new Stack<char>();
                char lastChar = '\0';

                foreach (char c in expression)
                {
                    if (char.IsWhiteSpace(c)) continue;
                    if (char.IsDigit(c) || char.IsLetter(c))
                    {
                        lastChar = c;
                        continue;
                    }
                    if (c == '(')
                    {
                        stack.Push(c);
                        lastChar = c;
                        continue;
                    }
                    if (c == ')')
                    {
                        if (stack.Count == 0 || "+-*/".Contains(lastChar))
                        {
                            return false;
                        }
                        stack.Pop();
                        lastChar = c;
                        continue;
                    }
                    if ("+-*/=".Contains(c))
                    {
                        if (lastChar == '\0' || lastChar == '(' || "+-*/=".Contains(lastChar))
                        {
                            if (c == '-' && (lastChar == '\0' || lastChar == '('))
                            {
                                lastChar = c;
                                continue;
                            }
                            return false;
                        }
                        lastChar = c;
                        continue;
                    }

                    return false;
                }

                return stack.Count == 0 && !"+-*/=".Contains(lastChar);
            }

            public static double EvaluateExpression(string expression)
            {
                var tokens = Regex.Matches(expression, @"\d+|[a-zA-Z]+|[+\-*/()]");
                var values = new Stack<double>();
                var ops = new Stack<string>();

                var precedence = new Dictionary<string, int>
                {
                    { "+", 1 },
                    { "-", 1 },
                    { "*", 2 },
                    { "/", 2 }
                };

                void ApplyOperator()
                {
                    var right = values.Pop();
                    var left = values.Count > 0 ? values.Pop() : 0;
                    var op = ops.Pop();

                    switch (op)
                    {
                        case "+": values.Push(left + right); break;
                        case "-": values.Push(left - right); break;
                        case "*": values.Push(left * right); break;
                        case "/":
                            if (right == 0)
                            {
                                throw new DivideByZeroException("Error: Division by zero");
                            }
                            values.Push(left / right);
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown operator: {op}");
                    }
                }

                foreach (Match token in tokens)
                {
                    string t = token.Value;

                    if (double.TryParse(t, out double number))
                    {
                        values.Push(number);
                    }
                    else if (char.IsLetter(t[0]))
                    {
                        if (variables.TryGetValue(t, out double variableValue))
                        {
                            values.Push(variableValue);
                        }
                        else
                        {
                            throw new KeyNotFoundException($"Undefined variable: {t}");
                        }
                    }
                    else if (t == "(")
                    {
                        ops.Push(t);
                    }
                    else if (t == ")")
                    {
                        while (ops.Count > 0 && ops.Peek() != "(")
                        {
                            ApplyOperator();
                        }
                        if (ops.Count == 0)
                        {
                            throw new InvalidOperationException("Mismatched parentheses");
                        }
                        ops.Pop();
                    }
                    else if (precedence.ContainsKey(t))
                    {
                        var valCount = -1;
                        var opsCount = -1;

                        if (values.Count == 0)
                        {
                            valCount = 0;
                        }
                        if (ops.Count == 0)
                        {
                            opsCount = 0;
                        }

                        if (t == "-" && (valCount == 0 || (opsCount > 0 && ops.Peek() == "(")))
                        {
                            values.Push(0);
                        }

                        while (ops.Count > 0 && precedence.ContainsKey(ops.Peek()) && precedence[ops.Peek()] >= precedence[t])
                        {
                            ApplyOperator();
                        }
                        ops.Push(t);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid token: {t}");
                    }
                }

                while (ops.Count > 0)
                {
                    ApplyOperator();
                }

                return values.Pop();
            }

            public static string Evaluate(string codeToExecute)
            {
                var parsedExpression = ParseExpression(codeToExecute);

                if (parsedExpression.Contains('='))
                {
                    var parts = parsedExpression.Split('=');
                    if (parts.Length != 2) return "Error: Invalid assignment";

                    var variable = parts[0].Trim();
                    var expression = parts[1].Trim();

                    if (!Regex.IsMatch(variable, @"^[a-zA-Z]\w*$"))
                    {
                        return "Error: Invalid variable name";
                    }

                    if (!IsValidExpression(expression))
                    {
                        return "Error: Invalid Expression";
                    }

                    try
                    {
                        var result = EvaluateExpression(expression);
                        variables[variable] = result;
                        return $"{variable}={result}";
                    }
                    catch (Exception ex)
                    {
                        return "Error: " + ex.Message;
                    }
                }
                else
                {
                    if (!IsValidExpression(parsedExpression))
                    {
                        return "Error: Invalid Expression";
                    }

                    try
                    {
                        var result = EvaluateExpression(parsedExpression);
                        return $"{result}";
                    }
                    catch (Exception ex)
                    {
                        return "Error: " + ex.Message;
                    }
                }
            }
        }
    }
}
