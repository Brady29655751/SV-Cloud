using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Parser {
    public static void ParseOptions(this Dictionary<string, string> dict, string _data) {
        _data = _data.Trim();
        if (_data == "none")
            return;

        var _options = _data.Split(new char[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < _options.Length; i++) {
            var result = _options[i].Split(new char[] {'='}, StringSplitOptions.RemoveEmptyEntries);
            if (result.Length != 2) {
                Debug.LogError("Option parsing failure.");
                continue;
            }
            dict.Add(result[0], result[1]);
        }
    }

    public static void ParseMultipleCondition(this List<List<ICondition>> condList, string _data) {
        _data = _data.Trim();
        if (_data == "none") {
            condList.Add(new List<ICondition>());
            return;
        }
        string[] _options_list = _data.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < _options_list.Length; i++) {
            condList.Add(ParseCondtionList(_options_list[i]));
        }
    }

    public static List<ICondition> ParseCondtionList(string data) {
        var conditions = new List<ICondition>();
        var _options = data.Split(new char[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < _options.Length; i++) {
            foreach (var op in Operator.condDict.Keys) {
                int index = _options[i].IndexOf(op);
                if (index > 0) {
                    var result = _options[i].Split(new string[] { op }, StringSplitOptions.RemoveEmptyEntries);
                    if (result.Length != 2) {
                        Debug.LogError("Option parsing failure.");
                        continue;
                    }
                    conditions.Add(new ICondition(op, result[0], result[1]));
                    break;
                }
            }
        }
        return conditions;
    }

    public static bool IsCondition(string cond) {
        return Operator.condDict.ContainsKey(cond);
    }

    public static bool IsOperator(string op) {
        return Operator.opDict.ContainsKey(op);
    }

    public static bool IsFraction(string frac) {
        string[] num = frac.Split('/');
        if (num.Length != 2)
            return false;
        int _;
        return (int.TryParse(num[0], out _) && (int.TryParse(num[1], out _)));
    }

    public static KeyValuePair<DataType, object> ParseDataType(string data) {
        int resint; 
        float resfloat;

        if (string.IsNullOrEmpty(data))
            return new KeyValuePair<DataType, object>(DataType.Null, data);

        if (IsCondition(data))
            return new KeyValuePair<DataType, object>(DataType.Condition, Operator.condDict.Get(data));

        if (IsOperator(data))
            return new KeyValuePair<DataType, object>(DataType.Operator, Operator.opDict.Get(data));

        if (IsFraction(data))
            return new KeyValuePair<DataType, object>(DataType.Fraction, ParseFraction(data));

        if (int.TryParse(data, out resint))
            return new KeyValuePair<DataType, object>(DataType.Int, resint);

        if (float.TryParse(data, out resfloat))
            return new KeyValuePair<DataType, object>(DataType.Float, resfloat);

        return new KeyValuePair<DataType, object>(DataType.Text, data);
    }

    public static KeyValuePair<int, int> ParseFraction(string frac) {
        string[] num = frac.Split('/');
        int a = int.Parse(num[0]);
        int b = int.Parse(num[1]);
        return new KeyValuePair<int, int>(a, b);
    }
    
    public static int ParseEffectExpression(string expr, Effect effect, BattleState state) {
        bool negativeFirst = expr.StartsWith("-");

        if (negativeFirst) {
            expr = expr.Substring(1);
        }

        string[] id = expr.Split(Operator.opDict.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        int value = Identifier.GetIdentifier(id[0], effect, state) * (negativeFirst ? -1 : 1);
        
        if (id.Length == 1)
            return value;

        for (int i = 1, opStartIdx = id[0].Length, opEndIdx = 0; i < id.Length; i++) {
            opEndIdx = expr.IndexOf(id[i], opStartIdx);
            string op = expr.Substring(opStartIdx, opEndIdx - opStartIdx);
            value = Operator.Operate(op, value, Identifier.GetIdentifier(id[i], effect, state));
            opStartIdx = opEndIdx + id[i].Length;
        }
        return value;
    }
    
}
