using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PortalSearchModel : IMonoBehaviour
{
    public CardFilter filter = new CardFilter(1) { zone = (int)CardZone.Engineering };

    public void Clear() {
        filter = new CardFilter(1){ zone = (int)CardZone.Engineering };
    }

    public void SetBool(string which, bool item) {
        filter.SetBool(which, item);
    }

    public void SetInt(string which, int item) {
        filter.SetInt(which, item);
    }

    public void SetString(string which, string input) {
        filter.SetString(which, input);
    }

    public void SelectInt(string which, int item) {
        filter.SelectInt(which, item);
    }

}
