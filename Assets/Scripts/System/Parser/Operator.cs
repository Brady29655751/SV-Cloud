using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Operator {
    public static Dictionary<string, Func<int, int, bool>> condDict { get; } = new Dictionary<string, Func<int, int, bool>>() {
        {"<", LessThan},
        {">", GreaterThan},
        {"=", Equal},
        {"[LTE]", LessThanOrEqual},
        {"[GTE]", GreaterThanOrEqual},
        {"[NOT]", NotEqual}
    };
    public static bool Condition(string op, int lhs, int rhs) {
        return condDict.ContainsKey(op) ? condDict.Get(op).Invoke(lhs, rhs) : false;
    }

    public static bool LessThan(int lhs, int rhs) {
        return lhs < rhs;
    }

    public static bool GreaterThan(int lhs, int rhs) {
        return lhs > rhs;
    }

    public static bool Equal(int lhs, int rhs) {
        return lhs == rhs;
    }

    public static bool LessThanOrEqual(int lhs, int rhs) {
        return LessThan(lhs, rhs) || Equal(lhs, rhs);
    }

    public static bool GreaterThanOrEqual(int lhs, int rhs) {
        return GreaterThan(lhs, rhs) || Equal(lhs, rhs);
    }

    public static bool NotEqual(int lhs, int rhs) {
        return lhs != rhs;
    }

    public static Dictionary<string, Func<int, int, int>> opDict { get; } = new Dictionary<string, Func<int, int, int>>() {
        {"+", Add},  {"-", Sub},  {"*", Mult},  {"/", Div},  {"^", Pow},  {"%", Mod},  
        {"[MIN]", Mathf.Min},     {"[MAX]", Mathf.Max},      {"[SET]", Set}, 
    };

    public static int Operate(string op, int lhs, int rhs) {
        return opDict.ContainsKey(op) ? opDict.Get(op).Invoke(lhs, rhs) : 0;
    }

    public static int Add(int lhs, int rhs) {
        return lhs + rhs;
    }
    public static int Sub(int lhs, int rhs) {
        return lhs - rhs;
    }
    public static int Mult(int lhs, int rhs) {
        return lhs * rhs;
    }
    public static int Div(int lhs, int rhs) {
        return Mathf.CeilToInt(lhs * 1f / rhs);
    }
    public static int Pow(int lhs, int rhs) {
        return (int)Mathf.Pow(lhs, rhs);
    }
    public static int Mod(int lhs, int rhs) {
        return lhs % rhs;
    }
    public static int Set(int lhs, int rhs) {
        return rhs;
    }

    public static ModifyOption ToModifyOption(this string option, ModifyOption defaultReturn = ModifyOption.Clear) {
        return option.ToLower() switch {
            "clear"     => ModifyOption.Clear,
            "add"       => ModifyOption.Add,
            "remove"    => ModifyOption.Remove,
            "set"       => ModifyOption.Set,
            _           => defaultReturn,
        };
    }
}

public enum DataType {
    Null,
    Text,
    Condition,
    Operator,
    Int,
    Float,
    Fraction,
}

public class ICondition 
{
    public string op, lhs, rhs;
    public ICondition(string op, string lhs, string rhs) {
        this.op = op;
        this.lhs = lhs;
        this.rhs = rhs;
    }
}

public enum ModifyOption {
    Clear,
    Set,
    Add,
    Remove,
}