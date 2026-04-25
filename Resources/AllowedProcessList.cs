using System.Collections.Generic;
using System.Linq;

public static class AllowedProcessList
{
  // Hardcoded list bundled inside the EXE
  private static readonly string[] _names =
  {
        "Roblox",
        "Steam"
    };

  // Fast lookup (case-insensitive)
  public static readonly HashSet<string> Names =
      _names
          .Select(n => n.ToLowerInvariant())
          .ToHashSet();
}
