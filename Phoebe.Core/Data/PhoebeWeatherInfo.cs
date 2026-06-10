namespace Phoebe;

public record PhoebeWeatherInfo(
	string WeatherName,
	int WeatherVariable1,
	int WeatherVariable2,
	PhoebeColor WeatherVariableColor
);