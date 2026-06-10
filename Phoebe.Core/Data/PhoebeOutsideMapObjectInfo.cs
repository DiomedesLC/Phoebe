namespace Phoebe;

public record PhoebeOutsideMapObjectInfo(string hazardName, PhoebeCurve spawnCurve) : PhoebeMapObjectInfo(hazardName, spawnCurve);