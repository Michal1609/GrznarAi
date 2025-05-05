using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GrznarAi.Web.Models
{
    public class CsvHistoryModel
    {
        public DateTime Date { get; set; }
        public float? TemperatureIn { get; set; }
        public float? TemperatureOut { get; set; }
        public float? Chill { get; set; }
        public float? DewIn { get; set; }
        public float? DewOut { get; set; }
        public float? HeatIn { get; set; }
        public float? Heat { get; set; }
        public float? HumidityIn { get; set; }
        public float? HumidityOut { get; set; }
        public float? WindSpeedHi { get; set; }
        public float? WindSpeedAvg { get; set; }
        public float? WindDirection { get; set; }
        public float? Bar { get; set; }
        public float? Rain { get; set; }
        public float? RainRate { get; set; }
        public float? SolarRad { get; set; }
        public int? Uvi { get; set; }

        public static CsvHistoryModel ParseFromCsvLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                throw new FormatException("CSV řádek je prázdný");
            }
            
            // Ošetření nestandardních znaků v řádku (např. odpovídající chybě v souboru 1?012,4)
            line = line.Replace("?", "");
            
            // Připravíme regex pro rozdělení CSV řádku
            var csvLine = line.Trim();
            var values = Regex.Split(csvLine, ";");

            // Doplníme chybějící hodnoty, pokud je potřeba
            var paddedValues = new string[18];
            for (int i = 0; i < paddedValues.Length; i++)
            {
                paddedValues[i] = (i < values.Length) ? values[i] : string.Empty;
            }

            try
            {
                return new CsvHistoryModel
                {
                    Date = ParseDateTime(paddedValues[0]),
                    TemperatureIn = ParseFloat(paddedValues[1]),
                    TemperatureOut = ParseFloat(paddedValues[2]),
                    Chill = ParseFloat(paddedValues[3]),
                    DewIn = ParseFloat(paddedValues[4]),
                    DewOut = ParseFloat(paddedValues[5]),
                    HeatIn = ParseFloat(paddedValues[6]),
                    Heat = ParseFloat(paddedValues[7]),
                    HumidityIn = ParseFloat(paddedValues[8]),
                    HumidityOut = ParseFloat(paddedValues[9]),
                    WindSpeedHi = ParseFloat(paddedValues[10]),
                    WindSpeedAvg = ParseFloat(paddedValues[11]),
                    WindDirection = ParseFloat(paddedValues[12]),
                    Bar = ParseFloat(paddedValues[13]),
                    Rain = ParseFloat(paddedValues[14]),
                    RainRate = ParseFloat(paddedValues[15]),
                    SolarRad = ParseFloat(paddedValues[16]),
                    Uvi = ParseInt(paddedValues[17])
                };
            }
            catch (FormatException ex)
            {
                throw new FormatException($"Chyba při zpracování CSV řádku: {ex.Message} - Řádek: {line}", ex);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Neočekávaná chyba při zpracování CSV řádku: {ex.Message} - Řádek: {line}", ex);
            }
        }

        private static DateTime ParseDateTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException("Datum je prázdné");
            }

            // Podrobné logovací informace
            Console.WriteLine($"Pokus o parsování datumu: '{value}', délka: {value.Length}, bytes: {BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(value))}");

            // Očištění hodnoty, odstranění neviditelných a problematických znaků
            string cleanValue = value.Trim();
            
            // Odstranění potenciálně problematických znaků
            cleanValue = Regex.Replace(cleanValue, @"[^\d\-:/ \.]", "");
            cleanValue = Regex.Replace(cleanValue, @"\s+", " ").Trim();
            
            Console.WriteLine($"Očištěná hodnota: '{cleanValue}', délka: {cleanValue.Length}, bytes: {BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(cleanValue))}");

            // Explicitní parsování pro rok-měsíc-den hodina:minuta:sekunda
            if (Regex.IsMatch(cleanValue, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$"))
            {
                try {
                    int year = int.Parse(cleanValue.Substring(0, 4));
                    int month = int.Parse(cleanValue.Substring(5, 2));
                    int day = int.Parse(cleanValue.Substring(8, 2));
                    int hour = int.Parse(cleanValue.Substring(11, 2));
                    int minute = int.Parse(cleanValue.Substring(14, 2));
                    int second = int.Parse(cleanValue.Substring(17, 2));
                    
                    return new DateTime(year, month, day, hour, minute, second);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Chyba při manuálním parsování data: {ex.Message}");
                }
            }
            
            // Seznam formátů, které se pokusíme rozpoznat
            string[] formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "dd.MM.yyyy HH:mm:ss",
                "dd.MM.yyyy HH:mm",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/MM/dd HH:mm"
            };

            // Pokusíme se o parsování s různými formáty
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(cleanValue, format, CultureInfo.InvariantCulture, 
                                         DateTimeStyles.None, out DateTime result))
                {
                    Console.WriteLine($"Úspěšné parsování s formátem: {format}");
                    return result;
                }
            }

            // Pokud formáty nevyhovují, zkusíme obecné parsování s invariantní a lokální kulturou
            if (DateTime.TryParse(cleanValue, CultureInfo.InvariantCulture, 
                                 DateTimeStyles.None, out DateTime result1))
            {
                Console.WriteLine($"Úspěšné parsování s CultureInfo.InvariantCulture");
                return result1;
            }

            if (DateTime.TryParse(cleanValue, CultureInfo.CurrentCulture, 
                                 DateTimeStyles.None, out DateTime result2))
            {
                Console.WriteLine($"Úspěšné parsování s CultureInfo.CurrentCulture");
                return result2;
            }

            // Pokud se nepodařilo zparsovat datum, vyhodíme výjimku
            Console.WriteLine($"Nepodařilo se zparsovat datum '{value}'");
            throw new FormatException($"Nepodařilo se zparsovat datum: '{value}' (očištěno na '{cleanValue}')");
        }

        private static float? ParseFloat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // Čištění hodnoty od nestandardních znaků
            string cleanValue = value.Trim();
            
            // Odstranění problematických znaků ('?', ' ', atd.)
            cleanValue = Regex.Replace(cleanValue, @"[^\d\.,\-]", "");
            
            // Převod na float (podpora českého i anglického formátu)
            cleanValue = cleanValue.Replace(',', '.');
            
            // Pokud by po čištění vznikly duplicitní desetinné čárky, necháme jen první
            int dotIndex = cleanValue.IndexOf('.');
            if (dotIndex >= 0)
            {
                int secondDotIndex = cleanValue.IndexOf('.', dotIndex + 1);
                if (secondDotIndex >= 0)
                {
                    cleanValue = cleanValue.Remove(secondDotIndex, 1);
                }
            }

            if (float.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }

            // Logovací informace pro debugging
            Console.WriteLine($"Nepodařilo se převést '{value}' (očištěno na '{cleanValue}') na číslo");
            return null;
        }

        private static int? ParseInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // Čištění hodnoty od nestandardních znaků
            string cleanValue = value.Trim();
            cleanValue = Regex.Replace(cleanValue, @"[^\d\-]", "");

            if (int.TryParse(cleanValue, out int result))
            {
                return result;
            }

            // Logovací informace pro debugging
            Console.WriteLine($"Nepodařilo se převést '{value}' (očištěno na '{cleanValue}') na celé číslo");
            return null;
        }
    }
} 