using System;
using UnityEngine;

public interface IProjectLinkSource
{
    public Uri BaseURI { get; }
    public string Key { get; }
}
