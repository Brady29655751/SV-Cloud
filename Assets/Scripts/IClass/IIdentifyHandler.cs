using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIdentifyHandler
{
    public int Id { get; }

    public bool TryGetIdenfier(string id, out int value);
    public int GetIdentifier(string id);
    public void SetIdentifier(string id, int value);
}
