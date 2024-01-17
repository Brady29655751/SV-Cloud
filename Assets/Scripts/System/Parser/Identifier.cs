using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Identifier {

    public static float GetIdentifier(string id, Effect effect, BattleState state) {
        var trimId = string.Empty;
        var lhsUnit = effect.invokeUnit;
        var rhsUnit = state.GetRhsUnitById(lhsUnit.id);
        
        if (id.TryTrimStart("lastEffect.", out trimId))
            return effect.GetLastEffectIdentifier(trimId, state);

        if (id.TryTrimStart("effect.", out trimId))
            return effect.GetIdentifier(trimId);
        
        if (id.TryTrimStart("me.", out trimId))
            return lhsUnit.GetIdentifier(trimId);

        if (id.TryTrimStart("op.", out trimId))
            return rhsUnit.GetIdentifier(trimId);

        return GetNumIdentifier(id);
    }

    public static float GetNumIdentifier(string id) {
        string trimId;
        float num = 0;

        if (id.TryTrimStart("random", out trimId)) {
            if (trimId == string.Empty)
                return Random.Range(0, 100);

            int startIndex = trimId.IndexOf('[');
            int middleIndex = trimId.IndexOf('~');
            int endIndex = trimId.IndexOf(']');
            string startExpr = trimId.Substring(startIndex + 1, middleIndex - startIndex - 1);
            string endExpr = trimId.Substring(middleIndex + 1, endIndex - middleIndex - 1);
            int startRange = int.Parse(startExpr);
            int endRange = int.Parse(endExpr);

            return Random.Range(startRange, endRange + 1);
        }
        return float.TryParse(id, out num) ? num : 0;
    }

}
