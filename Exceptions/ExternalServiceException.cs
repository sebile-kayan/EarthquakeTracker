namespace EarthquakeTracker.Exceptions;

// Dış bir servise (USGS) ulaşılamadığında veya cevabı okunamadığında fırlatılır.
// Message kullanıcıya gösterilebilecek kadar anlaşılır olmalı.
public class ExternalServiceException : Exception
{
	public ExternalServiceException(string message, Exception? innerException = null)
		: base(message, innerException)
	{
	}
}