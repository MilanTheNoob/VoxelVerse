using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// The structure object that stores a bunch of blocks ... Im struggling to find something to say about it
/// </summary>
[CreateAssetMenu]
public class StructureObj : ScriptableObject
{
    public List<StructureBlockClass> Blocks;
}
