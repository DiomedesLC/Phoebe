namespace Phoebe;

public record PhoebeInsideMapObjectInfo(string hazardName, PhoebeCurve spawnCurve, List<string> InteriorsDisallowedIn) : PhoebeMapObjectInfo(hazardName, spawnCurve);