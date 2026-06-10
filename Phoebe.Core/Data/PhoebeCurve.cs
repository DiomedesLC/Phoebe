namespace Phoebe;

public record PhoebeCurve(List<PhoebeKeyFrame> KeyFrames) {
	public float Evaluate(float time) {
		if(KeyFrames.Count == 0) {
			return 0f;
			// throw new Exception("Curve has no keyframes!");
		}

		PhoebeKeyFrame relevantKeyFrame = KeyFrames[0];
		PhoebeKeyFrame? nextKeyFrame = null;
		if(KeyFrames.Count > 1) {
			nextKeyFrame = KeyFrames[1];
		}

		for(int i = 0; i < KeyFrames.Count; i++) {
			PhoebeKeyFrame comparingKeyFrame = KeyFrames[i];
			if(comparingKeyFrame.time <= time) {
				relevantKeyFrame = comparingKeyFrame;
				if(i + 1 < KeyFrames.Count) {
					nextKeyFrame = KeyFrames[i + 1];
				}
			} else {
				break;
			}
		}

		if(relevantKeyFrame.time == time) {
			return relevantKeyFrame.value;
		}

		float valueAverage = relevantKeyFrame.value;
		if(nextKeyFrame != null) {
			valueAverage = (relevantKeyFrame.value + nextKeyFrame.value) / 2f;
		}
		return valueAverage;
	}
}