// See https://aka.ms/new-console-template for more information

using System.Collections;
using Speckle.Core.Models;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Speckle.Core.Api;
using Speckle.Core.Models;

// Note: some boilerplate code removed.

// Receive a revit commit (note: you will need a local account on speckle.xyz for this to work!)
var data = Helpers.Receive("https://speckle.xyz/streams/33c4891d20/commits/1a1552ea78").Result;
var flatData = data.Flatten().ToList();

var timberWalls = flatData.FindAll(obj => obj is Objects.BuiltElements.Revit.RevitWall wall && wall.type == "Wall - Timber Clad");

var windows = flatData.FindAll(obj => (string)obj["category"] == "Windows");


var rooms = flatData.FindAll(obj => obj is Objects.BuiltElements.Room);


// Note: to get only the unique levels, we need to de-duplicate them.
var levels = flatData.FindAll(obj => obj is Objects.BuiltElements.Level).Cast<Objects.BuiltElements.Level>().GroupBy(level => level.name).Select(g => g.First()).ToList();


var elementsByLevel = flatData.FindAll(obj => obj["level"] != null).GroupBy(obj => ((Base)obj["level"])["name"]);
foreach(var grouping in elementsByLevel) {
    Console.WriteLine($"On level {grouping.Key} there are {grouping.Count()} elements.");
}



public static class Extensions
{
    // Flattens a base object into all its constituent parts.
    public static IEnumerable<Base> Flatten(this Base obj)
    {
        yield return obj;

        var props = obj.GetDynamicMemberNames();
        foreach (var prop in props)
        {
            var value = obj[prop];
            if (value == null) continue;

            if (value is Base b)
            {
                var nested = b.Flatten();
                foreach (var child in nested) yield return child;
            }

            if (value is IDictionary dict)
            {
                foreach (var dictValue in dict.Values)
                {
                    if (dictValue is Base lb)
                    {
                        foreach (var lbChild in lb.Flatten()) yield return lbChild;
                    }
                }
            }

            if (value is IEnumerable enumerable)
            {
                foreach (var listValue in enumerable)
                {
                    if (listValue is Base lb)
                    {
                        foreach (var lbChild in lb.Flatten()) yield return lbChild;
                    }
                }
            }
        }
    }
}
